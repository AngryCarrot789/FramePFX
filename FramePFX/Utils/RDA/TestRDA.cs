// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see tthttps://www.gnu.org/licenses/tt.
// 

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.Utils.RDA
{
    public abstract class CompiledRapidDispatchActionExBase
    {
        [StructLayout(LayoutKind.Auto)]
        [CompilerGenerated]
        private struct DoExecuteAsyncMachine : IAsyncStateMachine
        {
            public int myState;
            public AsyncVoidMethodBuilder myBuilder;
            public CompiledRapidDispatchActionExBase rda;
            private TaskAwaiter myAwaiter;

            private void MoveNext()
            {
                int myCurrentState = this.myState;
                CompiledRapidDispatchActionExBase myRda = this.rda;
                try
                {
                    if (myCurrentState != 0)
                    {
                        object stateLock = myRda.stateLock;
                        bool lockTaken = false;
                        try
                        {
                            Monitor.Enter(stateLock, ref lockTaken);
                            myRda.state = 1;
                        }
                        finally
                        {
                            if (myCurrentState < 0 && lockTaken)
                            {
                                Monitor.Exit(stateLock);
                            }
                        }
                    }

                    int state = default;
                    try
                    {
                        TaskAwaiter awaiter;
                        if (myCurrentState != 0)
                        {
                            awaiter = myRda.ExecuteCore().GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                myCurrentState = (this.myState = 0);
                                this.myAwaiter = awaiter;
                                this.myBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                                return;
                            }
                        }
                        else
                        {
                            awaiter = this.myAwaiter;
                            this.myAwaiter = default;
                            myCurrentState = (this.myState = -1);
                        }

                        awaiter.GetResult();
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        if (myCurrentState < 0)
                        {
                            object stateLock = myRda.stateLock;
                            bool lockTaken = false;
                            try
                            {
                                Monitor.Enter(stateLock, ref lockTaken);
                                int daState = (state = myRda.state);
                                if (daState != 1)
                                {
                                    myRda.state = daState == 3 ? 2 : 0;
                                }
                                else
                                {
                                    myRda.state = 0;
                                }
                            }
                            finally
                            {
                                if (myCurrentState < 0 && lockTaken)
                                {
                                    Monitor.Exit(stateLock);
                                }
                            }
                        }
                    }

                    if (state == 3)
                    {
                        myRda.ScheduleExecute();
                    }
                }
                catch (Exception exception)
                {
                    this.myState = -2;
                    this.myBuilder.SetException(exception);
                    return;
                }

                this.myState = -2;
                this.myBuilder.SetResult();
            }

            void IAsyncStateMachine.MoveNext() => this.MoveNext();

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.myBuilder.SetStateMachine(stateMachine);
            }

            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            {
                //ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
                this.SetStateMachine(stateMachine);
            }
        }

        private const int STATE_NOT_SCHEDULED = 0;

        private const int STATE_RUNNING = 1;

        private const int STATE_SCHEDULED = 2;

        private const int STATE_RESCHEDULED = 3;

        private readonly string debugId;

        private volatile int state;

        private readonly object stateLock;

        private readonly Action doExecuteCallback;

        protected CompiledRapidDispatchActionExBase(string debugId)
        {
            this.debugId = debugId;
            this.stateLock = new object();
            this.doExecuteCallback = new Action(this.DoExecuteAsync);
        }

        [AsyncStateMachine(typeof(DoExecuteAsyncMachine))]
        private void DoExecuteAsync()
        {
            DoExecuteAsyncMachine stateMachine = default;
            stateMachine.myBuilder = AsyncVoidMethodBuilder.Create();
            stateMachine.rda = this;
            stateMachine.myState = -1;
            stateMachine.myBuilder.Start(ref stateMachine);
        }

        private void ScheduleExecute()
        {
        }

        protected bool BeginInvoke(Action actionInLock = null)
        {
            object obj = this.stateLock;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(obj, ref lockTaken);
                int num = this.state;
                if (num != 0)
                {
                    if (num == 1)
                    {
                        this.state = 3;
                        return true;
                    }

                    return false;
                }

                this.state = 2;
                if (actionInLock != null)
                {
                    actionInLock();
                }
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }

            this.ScheduleExecute();
            return true;
        }

        protected abstract Task ExecuteCore();
    }
}