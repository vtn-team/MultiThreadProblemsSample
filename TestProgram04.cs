using System.Runtime.InteropServices;

/// <summary>
/// パフォーマンスがどのようになるかを確認する
/// </summary>
static class TestProgram04
{
    struct TestDataA
    {
        public int Value1; // スレッドAが書き込む
        public int Value2; // スレッドBが書き込む
    };


    [StructLayout(LayoutKind.Explicit)]
    struct TestDataB
    {
        [FieldOffset(0)]
        public int Value1; // スレッドAが書き込む

        [FieldOffset(128)]
        public int Value2; // スレッドBが書き込む
    };

    static TestDataA dataA;
    static TestDataB dataB;
    static int TestDataC_Value1;
    static int TestDataC_Value2;

    //スレッドを作り処理を走らせる
    static Thread[] threads;

    static public void WarmupRunTest1(int coreNum)
    {
        threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                threads[i] = new Thread(new ThreadStart(() => WriteMultiThread(id)));
            }
        }
    }

    static public void WarmupRunTest2(int coreNum)
    {
        threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                threads[i] = new Thread(new ThreadStart(() => WriteMultiThreadAlignedStruct(id)));
            }
        }
    }

    static public void WarmupRunTest3(int coreNum)
    {
        threads = new Thread[coreNum];
        for (int i = 0; i < coreNum; ++i)
        {
            {
                int id = i;
                threads[i] = new Thread(new ThreadStart(() => WriteMultiThreadOtherData(id)));
            }
        }
    }

    /// <summary>
    /// 問題が特にあるわけではないが、遅い処理とされる。パフォーマンスを計測してみよう。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせよう</param>
    static public void RunTest1(int coreNum)
    {
        dataA.Value1 = 0;
        dataA.Value2 = 0;
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Start();
        }
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Join();
        }
    }

    /// <summary>
    /// 遅い処理をやや改善した。パフォーマンスを計測してみよう。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせつつ、偶数で実行すること</param>
    static public void RunTest2(int coreNum)
    {
        dataB.Value1 = 0;
        dataB.Value2 = 0;
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Start();
        }
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Join();
        }
    }

    /// <summary>
    /// 遅い処理をもう少し改善した。パフォーマンスを計測してみよう。
    /// </summary>
    /// <param name="coreNum">スレッド生成数。≒PCのコア数に合わせつつ、偶数で実行すること</param>
    static public void RunTest3(int coreNum)
    {
        TestDataC_Value1 = 0;
        TestDataC_Value2 = 0;

        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Start();
        }
        for (int i = 0; i < coreNum; ++i)
        {
            threads[i].Join();
        }
    }


    //ボスのタスク
    static void WriteMultiThread(int id)
    {
        for (int i = 0; i < 1000; ++i)
        {
            if (id % 2 == 0)
            {
                dataA.Value1++;
            }
            else
            {
                dataA.Value2++;
            }
        }
    }


    //配置済み
    static void WriteMultiThreadAlignedStruct(int id)
    {
        for (int i = 0; i < 1000; ++i)
        {
            if (id % 2 == 0)
            {
                dataB.Value1++;
            }
            else
            {
                dataB.Value2++;
            }
        }
    }


    //別データ
    static void WriteMultiThreadOtherData(int id)
    {
        for (int i = 0; i < 1000; ++i)
        {
            if (id % 2 == 0)
            {
                TestDataC_Value1++;
            }
            else
            {
                TestDataC_Value2++;
            }
        }
    }
}
