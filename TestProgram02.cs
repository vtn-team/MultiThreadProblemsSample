/// <summary>
/// ボスのHPがどのように変化し、マルチスレッドで実行するとき処理がどのようになるかを確認する
/// </summary>
class TestProgram02
{
    static object SyncObject = new object();
    static int BossHP = 100000000;  //テスト用にstaticにしている
    static bool IsShowMsg = false;

    /// <summary>
    /// シングルスレッドでの動作
    /// NOTE: パフォーマンスを見てみよう。
    /// </summary>
    static public void RunSingle(int coreNum)
    {
        BossAttackMultiThread(0);
    }

    /// <summary>
    /// 問題が起きる処理。どのように問題が起きるかは出力から確認する事。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせるとよい</param>
    /// <param name="msg">攻撃時のコンソールログを表示するか</param>
    static public void RunBad(int coreNum, bool msg)
    {
        BossHP = 100000;
        IsShowMsg = msg;

        //スレッドを作り処理を走らせる
        Thread[] threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                threads[i] = new Thread(new ThreadStart(() => BossAttackMultiThread(id)));
            }
        }
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Start();
        }
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Join();
        }
        /*
        //Taskでやるパターン
        List<Task> waitList = new List<Task>();
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                waitList.Add(Task.Run(() => BossAttackMultiThread(id)));
            }
        }

        Task.WhenAll(waitList).GetAwaiter().GetResult();
        */
    }

    /// <summary>
    /// 問題が起きない処理。どのように問題が解決したかを探ってみよう。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせるとよい</param>
    /// <param name="msg">攻撃時のコンソールログを表示するか</param>
    static public void RunOK(int coreNum, bool msg)
    {
        BossHP = 100000;
        IsShowMsg = msg;

        //スレッドを作り処理を走らせる
        Thread[] threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                threads[i] = new Thread(new ThreadStart(() => BossAttackMultiThreadSafe(id)));
            }
        }
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Start();
        }
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Join();
        }
        /*
        //Taskでやるパターン
        List<Task> waitList = new List<Task>();
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                waitList.Add(Task.Run(() => BossAttackMultiThread(id)));
            }
        }

        Task.WhenAll(waitList).GetAwaiter().GetResult();
        */
    }

    static void BossAttackMultiThread(int id)
    {
        Console.WriteLine($"{id}のプレイヤーが参加");
        Random random = new Random();
        while (BossHP > 0)
        {
            //ボスを倒していた
            if (BossHP <= 0)
            {
                Console.WriteLine($"{id}のプレイヤーが攻撃したがボスは倒されていた。");
                break;
            }

            // 体力をマイナス1～5する
            int PrevHP = BossHP;
            BossHP = BossHP - random.Next(1, 5);
            if (IsShowMsg) Console.WriteLine($"{id}のプレイヤーが攻撃した。攻撃後のHPは{BossHP}");

            //ボスを倒していた
            if (BossHP <= 0)
            {
                Console.WriteLine($"{id}のプレイヤーがボスを倒した。トドメを指す前のHPは{PrevHP}");
            }
        }
    }

    static void BossAttackMultiThreadSafe(int id)
    {
        Console.WriteLine($"{id}のプレイヤーが参加");
        Random random = new Random();
        while (BossHP > 0)
        {
            lock (SyncObject)
            {
                //ボスを倒していた
                if (BossHP <= 0)
                {
                    Console.WriteLine($"{id}のプレイヤーが攻撃したがボスは倒されていた。");
                    break;
                }

                // 体力をマイナス1～5する
                int PrevHP = BossHP;
                BossHP = BossHP - random.Next(1, 5);
                if (IsShowMsg) Console.WriteLine($"{id}のプレイヤーが攻撃した。攻撃後のHPは{BossHP}");

                //ボスを倒していた
                if (BossHP <= 0)
                {
                    Console.WriteLine($"{id}のプレイヤーがボスを倒した。トドメを指す前のHPは{PrevHP}");
                }
            }
        }
    }
}