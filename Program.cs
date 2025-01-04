using System.Diagnostics;

/// <summary>
/// マルチスレッドの動きを確認するためのプログラム
/// </summary>
static class MultiThreadTest
{
    static public void Main()
    {
        //自分のPCの論理コア(プロセッサ)数を入力する。
        int coreNum = 16;

        //適切な入力値は論理コア数と同じかそれ以下である。論理コア数は以下から参照できる。
        //「タスクマネージャー」を開く
        //→「パフォーマンス」タブを選択する
        //→「CPU」を選択する
        //記載されている「論理プロセッサ数」が該当する。


        //それぞれのプログラムをコメントアウトを外して実行すること。


        //テスト1： RACE CONDITION(コロプラ面接対策)
        //          {coreNum}回ListやQueueに配列を格納するプログラム。
        //          Listなどをマルチスレッドで操作するプログラムはあまり書かないが、気が付いたらそのような実装になってしまっていることもある。
        //          それぞれのプログラムがどのような結果になるかを確認してみよう。
        //          また、それぞれの関数がどのように違うのか、どう変えると問題が解決するのかを推察する事。テスト2は少し変えると挙動が変化する。

        //プログラム
        //TestProgram01.RunBad(coreNum);
        //TestProgram01.RunOK(coreNum);


        //テスト2： DAMAGE RACE (競合のゲーム的な実装)
        //          ボスを{coreNum}人で攻撃するプログラム。ネットワークゲームなどで見られる実装となる。
        //          それぞれのプログラムがどのような結果になるかを確認してみよう。
        //          その後、第二引数をtrueにして動きをより詳細に確認してみよう。
        //          また、それぞれの関数がどのように違うのか、どう変えると問題が解決するのかを推察する事。

        //プログラム
        //TestProgram02.RunBad(coreNum, false);
        //TestProgram02.RunOK(coreNum, false);


        //テスト3： DEAD LOCK
        //          ボスとプレイヤーを{coreNum}人で対決させるプログラム。ネットワークゲームなどで見られる実装となる。
        //          それぞれのプログラムがどのような結果になるかを確認してみよう。(※停止するのは正常)
        //          その後、第二引数をtrueにして動きをより詳細に確認してみよう。
        //          また、それぞれの関数がどのように違うのか、どう変えると問題が解決するのかを推察する事。

        //プログラム
        //TestProgram03.RunBad(coreNum, true);
        //TestProgram03.RunOK(coreNum, true);



        //テスト4： FALSE SHARING
        //          特定の変数を{coreNum}数のスレッドで書きこむプログラム。特に何もなさそうだが…パフォーマンスに差が出ています。
        //          あまり業務でこの書き方を意識することはないので…あっているかは厳密にはわかりませんが、これはとある概念を説明するうえで重要な現象です。
        //          別途授業で解説をします。
        //          以下のコードは、実行するのみで構いません。

        /*
        coreNum = 8;
        long elapsedMilliseconds = 0;
        double elapsedNanoseconds = 0.0f;
        for (int i = 0; i < 10; ++i)
        {
            using (var pf = new PerformanceCounter("RunTest1"))
            {
                TestProgram04.WarmupRunTest1(coreNum);
                pf.Start();
                TestProgram04.RunTest1(coreNum);
                pf.Stop();
                elapsedMilliseconds += pf.ElapsedMilliseconds;
                elapsedNanoseconds += pf.ElapsedNanoseconds;
            }
        }
        Console.WriteLine($"RunTest1の処理時間平均(ナノ秒/ミリ秒): {elapsedNanoseconds / 10} ns / {elapsedMilliseconds / 10} ms");

        elapsedMilliseconds = 0;
        elapsedNanoseconds = 0.0f;
        for (int i = 0; i < 10; ++i)
        {
            using (var pf = new PerformanceCounter("RunTest2"))
            {
                TestProgram04.WarmupRunTest2(coreNum);
                pf.Start();
                TestProgram04.RunTest2(coreNum);
                pf.Stop();
                elapsedMilliseconds += pf.ElapsedMilliseconds;
                elapsedNanoseconds += pf.ElapsedNanoseconds;
            }
        }
        Console.WriteLine($"RunTest2の処理時間平均(ナノ秒/ミリ秒): {elapsedNanoseconds / 10} ns / {elapsedMilliseconds / 10} ms");

        elapsedMilliseconds = 0;
        elapsedNanoseconds = 0.0f;
        for (int i = 0; i < 10; ++i)
        {
            using (var pf = new PerformanceCounter("RunTest3"))
            {
                TestProgram04.WarmupRunTest3(coreNum);
                pf.Start();
                TestProgram04.RunTest3(coreNum);
                pf.Stop();
                elapsedMilliseconds += pf.ElapsedMilliseconds;
                elapsedNanoseconds += pf.ElapsedNanoseconds;
            }
        }
        Console.WriteLine($"RunTest3の処理時間平均(ナノ秒/ミリ秒): {elapsedNanoseconds / 10} ns / {elapsedMilliseconds / 10} ms");
        */
    }

    /// <summary>
    /// 計測用のクラス
    /// NOTE: 今回の本質ではないので特に解説はしない
    /// </summary>
    class PerformanceCounter : IDisposable
    {
        // 高精度タイマーの周波数 (1秒あたりのタイマー刻み回数)
        static long frequency = Stopwatch.Frequency;
        static double nanosecondsPerTick = 1e9 / frequency; // 1ティックあたりのナノ秒

        bool _isCalc = false;
        string _message;
        double _elapsedNanoseconds;
        long _elapsedMilliseconds;
        Stopwatch _stopwatch = new Stopwatch();

        public double ElapsedNanoseconds => _elapsedNanoseconds;
        public long ElapsedMilliseconds => _elapsedMilliseconds;
        public PerformanceCounter(string msg) { _message = msg; }

        // 計測開始
        public PerformanceCounter Start()
        {
            _isCalc = true;
            _elapsedNanoseconds = 0.0f;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            return this;
        }

        public void Stop()
        {
            _isCalc = false;

            // 計測終了
            _stopwatch.Stop();

            // 経過時間を記録
            _elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

            // 経過時間をナノ秒に変換
            _elapsedNanoseconds = _stopwatch.ElapsedTicks * nanosecondsPerTick;
            Console.WriteLine($"{_message} - 処理時間(ナノ秒): {_elapsedNanoseconds} ns");
        }

        public void Dispose()
        {
            if (_isCalc) Stop();
        }
    }
}