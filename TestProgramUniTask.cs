/*
 * 思った通りの動きをしなかったため、UniTaskの仕様が変更されたか、Unity(IL2CPP)以外で動作させると意図しない動きをする可能性がある。
 * よってこれはコードのみ残しておく。
 * 
 * 本来であれば、スレッドIDが同一となる動き(非同期的実行)が確認できるはずであったが、適切にスレッドプールが使用されている事がわかる。
 * 
using Cysharp.Threading.Tasks;

/// <summary>
/// ボスのHPがどのように変化し、マルチスレッドで実行するとき処理がどのようになるかを確認する
/// </summary>
class TestProgramUniTask
{
    static object SyncObject = new object();
    static int BossHP = 100000000;  //テスト用にstaticにしている
    static bool IsShowMsg = false;

    /// <summary>
    /// 問題となる処理。どのように問題が起きるかは出力から確認する事。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせるとよい</param>
    /// <param name="msg">攻撃時のコンソールログを表示するか</param>
    static public void RunUniTask(int coreNum, bool msg)
    {
        Task.Run(async () =>
        {
            await RunAsync(coreNum, msg);
        }).GetAwaiter().GetResult();
    }

    static async Task RunAsync(int coreNum, bool msg)
    {
        BossHP = 100000;
        IsShowMsg = msg;

        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine($"スレッドIDは{threadId}");

        //スレッドを作り処理を走らせる
        List<UniTask> waitList = new List<UniTask>();
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                waitList.Add(UniTask.Run(() => BossAttackMultiThreadSafe(id)));
            }
        }
        await waitList;

        threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine($"スレッドIDは{threadId}");
    }

    static void BossAttackMultiThreadSafe(int id)
    {
        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine($"{id}のプレイヤーが参加。スレッドIDは{threadId}");
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
*/