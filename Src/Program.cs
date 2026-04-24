using System;
using System.CommandLine;
using System.Threading;
using Gamelab.Screens;
using Gamelab.Services.Sound;
using Gamelab.Utils;
using static Gamelab.GamelabGame;

namespace Gamelab;

public static class Program
{
    static GamelabGame game;
    static Thread cicdTestThread;
    static Logger logger = new("Program");

    [STAThread]
    static void Main(string[] args)
    {
#if DEBUG
        var runMode = RunMode.Debug;
#else
        var runMode = RunMode.Release;
#endif

        var cmd = new RootCommand();

        var runCICDTestOption = new Option<bool>("--run-cicd-test");
        cmd.Add(runCICDTestOption);

        var options = cmd.Parse(args);

        var runCICDTest = options.GetValue(runCICDTestOption);

        game = new GamelabGame(runMode, new DesktopAndMacNativeFmodLibrary());

        if (runCICDTest)
        {
            cicdTestThread = new Thread(RunCICDTest);
            cicdTestThread.Start();
        }

        game.Run();

        cicdTestThread?.Join();
        game.Dispose();
    }

    private static void RunCICDTest()
    {
        logger.Info("Started CICD Test");

        logger.Info("Starting Game...");

        while (!game.IsRunning)
        {
            Thread.Yield();
        }

        var screenFactories = AbstractGameScreen.GetScreenFactories();
        foreach (var screenFactory in screenFactories)
        {
            var screen = screenFactory.Instantiate(game);

            logger.Info($"Switching to screen \"{screen}\"...");

            game.SwitchToScreen(screen);

            while (!screen.IsActive || screen.NumFramesDrawn < 10)
            {
                Thread.Yield();
            }
        }

        game.Exit();
    }
}