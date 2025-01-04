using System.Collections.Concurrent;

/// <summary>
/// コレクションの値がどのようになるかを確認する
/// </summary>
static class TestProgram01
{
    /// <summary>
    /// シングルスレッドでの動作
    /// </summary>
    static public void RunSingle(int coreNum)
    {
        for (int i = 0; i < coreNum; ++i)
        {
            AddQueueMethod();
        }

        //結果発表
        Console.WriteLine("ListのCountは:" + queueTest.Count);
        Console.WriteLine("QueueのCountは:" + queueTest.Count);
    }

    /// <summary>
    /// 問題が起きる処理。どのように問題が起きるかは出力から確認する事。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせるとよい</param>
    static public void RunBad(int coreNum)
    {
        //配列数確保しておく
        //NOTE: コメントアウトするとどうなるかを確認するとよい
        queueTest = new Queue<int>(coreNum * 1000);

        //スレッドを作り処理を走らせる
        Thread[] threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                threads[i] = new Thread(new ThreadStart(AddQueueMethod));
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
                waitList.Add(Task.Run(() => AddQueueMethod(id)));
            }
        }

        Task.WhenAll(waitList).GetAwaiter().GetResult();
        */

        //結果発表
        Console.WriteLine("ListのCountは:" + queueTest.Count);
        Console.WriteLine("QueueのCountは:" + queueTest.Count);
    }

    /// <summary>
    /// 問題が起きない処理。どのように問題が解決したかを探ってみよう。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせるとよい</param>
    static public void RunOK(int coreNum)
    {
        //NOTE: Queueと何が違うかを比較して実行してみること
        queueTestSafe.Clear();

        //スレッドを作り処理を走らせる
        Thread[] threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                threads[i] = new Thread(new ThreadStart(AddQueueMethodSafe));
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
                waitList.Add(Task.Run(() => AddQueueMethodSafe(id)));
            }
        }

        Task.WhenAll(waitList).GetAwaiter().GetResult();
        */

        //結果発表
        Console.WriteLine("ListのCountは:" + queueTestSafe.Count);
        Console.WriteLine("QueueのCountは:" + queueTestSafe.Count);
    }


    //テストのためにstaticにしている
    static List<int> listTest = new List<int>();
    static Queue<int> queueTest = new Queue<int>();

    static public void AddQueueMethod()
    {
        for (int i = 0; i < 1000; ++i)
        {
            listTest.Add(i);
            queueTest.Enqueue(i);
        }
    }


    //テストのためにstaticにしている
    static ConcurrentBag<int> listTestSafe = new ConcurrentBag<int>();
    static ConcurrentQueue<int> queueTestSafe = new ConcurrentQueue<int>();

    static public void AddQueueMethodSafe()
    {
        for (int i = 0; i < 1000; ++i)
        {
            listTestSafe.Add(i);
            queueTestSafe.Enqueue(i);
        }
    }
}