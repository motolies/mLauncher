using System;
using System.ComponentModel;
using System.Threading;

namespace mUT
{
    public class ThreadWorker
    {
        
        Thread th;
        public event EventHandler DoWork;
        public event EventHandler OnCompleted;

        public event ProcessChangedEventHandler OnProcessChanged;
        public delegate void ProcessChangedEventHandler(object sender, ProgressEventArgs e);

        public void Stop()
        {
            th.Abort();
        }

        public void ReportProgress(float per)
        {
            if (OnProcessChanged != null)
                OnProcessChanged(this, new ProgressEventArgs(per));
        }

        public void Run()
        {
            th = new Thread(Work);
            th.IsBackground = true;
            th.Start();
        }

        private void Work()
        {
            if (DoWork != null)
                DoWork(this, EventArgs.Empty);

            if (OnCompleted != null)
                OnCompleted(this, EventArgs.Empty);
        }
    }
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(float percent)
        {
            this.Percent = percent;
        }

        public float Percent { get; private set; }
    }
}
