﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using System.Timers;
using System;
using System.Threading;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using static cs_winui_packaged.Kernel32;

namespace cs_winui_packaged
{
    public partial class S2_CrashRecovery : Page
    {
        private DispatcherTimer _timer;
        private uint _counter;

        public S2_CrashRecovery()
        {
            this.InitializeComponent();
            setRecoveredSecondsCount();

            // Register for crash and hang restart.
            Kernel32.RegisterApplicationRestart(_counter.ToString(), Kernel32.RestartRestrictions.NotOnPatch | Kernel32.RestartRestrictions.NotOnReboot);

            // Register recovery callback to update restart arguments with the latest seconds counter value.
            Kernel32.RegisterApplicationRecoveryCallback(new RecoveryDelegate(p =>
            {
                Kernel32.ApplicationRecoveryInProgress(out bool canceled);
                if (canceled)
                {
                    Environment.Exit(2);
                }

                Kernel32.RegisterApplicationRestart(_counter.ToString(), Kernel32.RestartRestrictions.NotOnPatch | Kernel32.RestartRestrictions.NotOnReboot);

                ApplicationRecoveryFinished(true);
                return 0;
            }), IntPtr.Zero, 5000, 0);

            startTimer(); 
        }

        private void setRecoveredSecondsCount()
        {
            string[] commandLineArguments = Environment.GetCommandLineArgs();
            if (commandLineArguments.Length > 1)
            {
                commandLineArguments = commandLineArguments.Skip(1).ToArray();
                try
                {
                    _counter = Convert.ToUInt32(commandLineArguments[0]);
                }
                catch
                {
                    _counter = 0;
                }
            }
            else
            {
                _counter = 0;
            }

            counterTextBlock.Text = _counter.ToString();
        }

        private void startTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += timer_TickEvent;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        private void timer_TickEvent(object sender, object e)
        {
            _counter++;
            counterTextBlock.Text = _counter.ToString();
        }

        private void crash_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 60; i >= 0; i--)
            {
                System.Threading.Thread.Sleep(1000);
            }

            throw new Exception("Fatal crash!");
        }
    }
}
