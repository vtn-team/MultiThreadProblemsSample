/// <summary>
/// ボスのHPがどのように変化し、マルチスレッドで実行するとき処理がどのようになるかを確認する
/// </summary>
class TestProgram03
{
    //テスト用にstaticにしている
    static object SyncObjectEnemy = new object();
    static object SyncObjectPlayer = new object();
    static int BossHP = 100000000;  
    static int PlayerHP = 100000000;
    static bool IsShowMsg = false;

    /// <summary>
    /// シングルスレッドでの動作
    /// NOTE: このテストはシングルスレッド化することはできない。
    /// </summary>
    static public void RunSingle(int coreNum)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 問題が起きる処理。どのように問題が起きるかは出力から確認する事。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせつつ、偶数で実行すること</param>
    /// <param name="msg">攻撃時のコンソールログを表示するか</param>
    static public void RunBad(int coreNum, bool msg)
    {
        BossHP = 100000;
        PlayerHP = 100000;
        IsShowMsg = msg;

        //スレッドを作り処理を走らせる
        Thread[] threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                if (id % 2 == 0)
                {
                    threads[i] = new Thread(new ThreadStart(() => BossMultiThread(id)));
                }
                else
                {
                    threads[i] = new Thread(new ThreadStart(() => PlayerMultiThread(id)));
                }
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
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせつつ、偶数で実行すること</param>
    /// <param name="msg">攻撃時のコンソールログを表示するか</param>
    static public void RunOK(int coreNum, bool msg)
    {
        BossHP = 100000;
        PlayerHP = 100000;
        IsShowMsg = msg;

        //スレッドを作り処理を走らせる
        Thread[] threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                if(id%2 == 0)
                {
                    threads[i] = new Thread(new ThreadStart(() => BossMultiThreadSafe(id)));
                }
                else
                {
                    threads[i] = new Thread(new ThreadStart(() => PlayerMultiThreadSafe(id)));
                }
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

    #region BadCase
    //ボスのタスク
    static void BossMultiThread(int id)
    {
        Console.WriteLine($"{id}のボスタスク");
        Random random = new Random();
        while (BossHP > 0)
        {
            //ボスを回復する
            lock (SyncObjectEnemy)
            {
                //死んでいた
                if (BossHP <= 0)
                {
                    Console.WriteLine($"{id}のボスタスクが行動しようとしたが死んでいた。");
                    break;
                }

                // 体力を0～3回復する
                BossHP = BossHP + random.Next(0, 3);
                if (IsShowMsg) Console.WriteLine($"{id}のボスタスクが回復した。回復後のHPは{BossHP}");

                //プレイヤーを攻撃する
                lock (SyncObjectPlayer)
                {
                    //プレイヤーを倒していた
                    if (PlayerHP <= 0)
                    {
                        Console.WriteLine($"{id}のボスタスクが攻撃したがプレイヤーは倒されていた。");
                        break;
                    }

                    // 体力をマイナス1～5する
                    int PrevHP = PlayerHP;
                    PlayerHP = PlayerHP - random.Next(1, 5);
                    if (IsShowMsg) Console.WriteLine($"{id}のボスタスクが攻撃した。攻撃後のPlayerHPは{PlayerHP}");

                    //ボスを倒していた
                    if (PlayerHP <= 0)
                    {
                        Console.WriteLine($"{id}のボスタスクがプレイヤーを倒した。トドメを指す前のHPは{PrevHP}");
                    }
                }
            }
            
        }
    }

    //プレイヤーのタスク
    static void PlayerMultiThread(int id)
    {
        Console.WriteLine($"{id}のプレイヤータスク");
        Random random = new Random();
        while (BossHP > 0)
        {
            //ボスを回復する
            lock (SyncObjectPlayer)
            {
                //死んでいた
                if (PlayerHP <= 0)
                {
                    Console.WriteLine($"{id}のプレイヤータスクが行動しようとしたが死んでいた。");
                    break;
                }

                // 体力を0～3回復する
                PlayerHP = PlayerHP + random.Next(0, 3);
                if (IsShowMsg) Console.WriteLine($"{id}のプレイヤータスクが回復した。回復後のHPは{PlayerHP}");

                //ボスを攻撃する
                lock (SyncObjectEnemy)
                {
                    //ボスを倒していた
                    if (BossHP <= 0)
                    {
                        Console.WriteLine($"{id}のプレイヤータスクが回復したがボスは倒されていた。");
                        break;
                    }

                    // 体力をマイナス1～5する
                    int PrevHP = BossHP;
                    BossHP = BossHP - random.Next(1, 5);
                    if (IsShowMsg) Console.WriteLine($"{id}のプレイヤータスクが攻撃した。攻撃後のHPは{BossHP}");

                    //ボスを倒していた
                    if (BossHP <= 0)
                    {
                        Console.WriteLine($"{id}のプレイヤータスクがボスを倒した。トドメを指す前のHPは{PrevHP}");
                    }
                }
            }
        }
    }
    #endregion


    #region GoodCase
    //ボスのタスク
    static void BossMultiThreadSafe(int id)
    {
        Console.WriteLine($"{id}のボスタスク");
        Random random = new Random();
        while (BossHP > 0)
        {
            //ボスを回復する
            lock (SyncObjectEnemy)
            {
                //死んでいた
                if (BossHP <= 0)
                {
                    Console.WriteLine($"{id}のボスタスクが行動しようとしたが死んでいた。");
                    break;
                }

                // 体力を0～3回復する
                BossHP = BossHP + random.Next(0, 3);
                if (IsShowMsg) Console.WriteLine($"{id}のボスタスクが回復した。回復後のHPは{BossHP}");
            }

            //プレイヤーを攻撃する
            lock (SyncObjectPlayer)
            {
                //プレイヤーを倒していた
                if (PlayerHP <= 0)
                {
                    Console.WriteLine($"{id}のボスタスクが攻撃したがプレイヤーは倒されていた。");
                    break;
                }

                // 体力をマイナス5～10する
                int PrevHP = PlayerHP;
                PlayerHP = PlayerHP - random.Next(5, 10);
                if (IsShowMsg) Console.WriteLine($"{id}のボスタスクが攻撃した。攻撃後のPlayerHPは{PlayerHP}");

                //ボスを倒していた
                if (PlayerHP <= 0)
                {
                    Console.WriteLine($"{id}のボスタスクがプレイヤーを倒した。トドメを指す前のHPは{PrevHP}");
                }
            }
        }
    }

    //プレイヤーのタスク
    static void PlayerMultiThreadSafe(int id)
    {
        Console.WriteLine($"{id}のプレイヤータスク");
        Random random = new Random();
        while (BossHP > 0)
        {
            //プレイヤーを回復する
            lock (SyncObjectPlayer)
            {
                //死んでいた
                if (PlayerHP <= 0)
                {
                    Console.WriteLine($"{id}のプレイヤータスクが行動しようとしたが死んでいた。");
                    break;
                }

                // 体力を0～3回復する
                PlayerHP = PlayerHP + random.Next(0, 3);
                if (IsShowMsg) Console.WriteLine($"{id}のプレイヤータスクが回復した。回復後のHPは{PlayerHP}");
            }

            //ボスを攻撃する
            lock (SyncObjectEnemy)
            {
                //ボスを倒していた
                if (BossHP <= 0)
                {
                    Console.WriteLine($"{id}のプレイヤータスクが回復したがボスは倒されていた。");
                    break;
                }

                // 体力をマイナス5～10する
                int PrevHP = BossHP;
                BossHP = BossHP - random.Next(5,10);
                if (IsShowMsg) Console.WriteLine($"{id}のプレイヤータスクが攻撃した。攻撃後のHPは{BossHP}");

                //ボスを倒していた
                if (BossHP <= 0)
                {
                    Console.WriteLine($"{id}のプレイヤータスクがボスを倒した。トドメを指す前のHPは{PrevHP}");
                }
            }
        }
    }
    #endregion
}