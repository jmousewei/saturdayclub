using mshtml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace saturdayclub.Analyze
{
    public class ActivityAnalyzer : AnalyzerBase
    {
        public const string DefaultPage = "http://www.niwota.com/quan/13142806";

        public ActivityAnalyzer()
        {

        }

        private void OnDocumentLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (string.Compare(e.Url.AbsoluteUri, DefaultPage, false) == 0)
            {
                List<string> activityList = new List<string>();
                WebBrowser browser = sender as WebBrowser;
                AnalyzeToken token = (AnalyzeToken)browser.Tag;
                try
                {
                    var dom = browser.Document.DomDocument as IHTMLDocument2;
                    var list = dom.all.item("activities") as IHTMLElement;
                    var divs = list.all.tags("div") as IHTMLElementCollection;
                    IHTMLElement body = null;
                    foreach (IHTMLElement div in divs)
                    {
                        if (string.Compare(div.className, "col_body", true) == 0)
                        {
                            body = div;
                            break;
                        }
                    }
                    var uls = body.all.tags("ul") as IHTMLElementCollection;
                    IHTMLElement ul = null;
                    foreach (IHTMLElement element in uls)
                    {
                        if (string.Compare(element.className, "activities txt_light", true) == 0)
                        {
                            ul = element;
                            break;
                        }
                    }
                    var lis = ul.all.tags("li") as IHTMLElementCollection;
                    foreach (IHTMLElement li in lis)
                    {
                        if (string.Compare(li.className, "head", true) == 0)
                        {
                            continue;
                        }
                        var spans = li.all.tags("span") as IHTMLElementCollection;
                        foreach (IHTMLElement span in spans)
                        {
                            if (string.Compare(span.className, "theme", true) == 0)
                            {
                                var anchor = span.all.tags("a")[0] as IHTMLElement;
                                activityList.Add(anchor.innerText.Trim());
                            }
                        }
                    }
                }
                catch
                { }
                SetResult(token, activityList);
                ((AutoResetEvent)token.Waitable).Set();
            }
        }

        protected override void Analyze(AnalyzeToken token)
        {
            WebBrowser browser = new WebBrowser();
            browser.DocumentCompleted += OnDocumentLoaded;
            browser.Tag = token;
            browser.Navigate(DefaultPage);
        }
    }

    public struct AnalyzeToken : IDisposable
    {
        private WaitHandle waitHandle;
        private object state;
        private IAnalyzer analyzer;

        public WaitHandle Waitable
        {
            get { return this.waitHandle; }
        }

        public object UserState
        {
            get { return this.state; }
        }

        public IAnalyzer Analyzer
        {
            get { return this.analyzer; }
        }

        public AnalyzeToken(WaitHandle waitable, object state, IAnalyzer analyzer)
            : this()
        {
            if (waitable == null)
                throw new ArgumentNullException("waitHandle");
            if (analyzer == null)
                throw new ArgumentNullException("analyzer");
            this.waitHandle = waitable;
            this.state = state;
            this.analyzer = analyzer;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.waitHandle.Dispose();
                this.waitHandle = null;
                this.state = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public static bool operator ==(AnalyzeToken left, AnalyzeToken right)
        {
            bool equivalent = object.ReferenceEquals(left.waitHandle, right.waitHandle) &&
                object.ReferenceEquals(left.state, right.state) &&
                object.ReferenceEquals(left.analyzer, right.analyzer);
            return equivalent;
        }

        public static bool operator !=(AnalyzeToken left, AnalyzeToken right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is AnalyzeToken)
            {
                return (((AnalyzeToken)obj) == this);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 37;
            hash = hash * 19 + this.waitHandle.GetHashCode();
            hash = hash * 19 + ((this.state == null) ? 0 : this.state.GetHashCode());
            hash = hash * 19 + this.analyzer.GetHashCode();
            return hash;
        }
    }

    public interface IAnalyzer
    {
        AnalyzeToken Analyze(object state);
        object RetrieveResult(AnalyzeToken token);
    }

    public abstract class AnalyzerBase : IAnalyzer
    {
        private readonly Dictionary<AnalyzeToken, object> resultTable;

        protected AnalyzerBase()
        {
            this.resultTable = new Dictionary<AnalyzeToken, object>();
        }

        public AnalyzeToken Analyze(object state)
        {
            var guarantee = CreateWaitHandle();
            var token = new AnalyzeToken(guarantee, state, this);
            Analyze(token);
            return token;
        }

        protected abstract void Analyze(AnalyzeToken token);

        public object RetrieveResult(AnalyzeToken token)
        {
            return RetrieveResult(token, Timeout.Infinite);
        }

        public object RetrieveResult(AnalyzeToken token, int timeout)
        {
            if (!token.Waitable.WaitOne(timeout))
            {
                throw new TimeoutException();
            }
            object result = null;
            if (!this.resultTable.TryGetValue(token, out result))
            {
                throw new InvalidOperationException();
            }
            this.resultTable.Remove(token);
            return result;
        }

        protected void SetResult(AnalyzeToken token, object result)
        {
            this.resultTable[token] = result;
        }

        protected virtual WaitHandle CreateWaitHandle()
        {
            return new AutoResetEvent(false);
        }
    }

    public interface IAnalyzeContext
    {
        void RunAsync(Action action);
        void Exit();
    }

    public sealed class AnalyzeContext : ApplicationContext, IAnalyzeContext
    {
        private Thread messenger;
        private bool closed;

        public AnalyzeContext()
        {
            this.messenger = null;
            this.closed = false;
        }

        public bool IsDisposed
        {
            get
            {
                return (this.messenger != null) && (!this.messenger.IsAlive);
            }
        }

        public void RunAsync(Action action)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);
            if (this.closed)
                throw new InvalidOperationException();
            if (this.messenger != null)
                return;
            this.messenger = new Thread(RunAsyncCore);
            this.messenger.IsBackground = true;
            this.messenger.SetApartmentState(ApartmentState.STA);
            this.messenger.Start(action);
        }

        private void RunAsyncCore(object state)
        {
            try
            {
                Action action = state as Action;
                if (action != null)
                {
                    action();
                    Application.Run(this);
                }
            }
            catch (ThreadInterruptedException) { }
            catch (ThreadAbortException) { }
        }

        public void RunAsync<T>(Action<T> action, T @object)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);
            if (this.closed)
                throw new InvalidOperationException();
            if (this.messenger != null)
                return;
            this.messenger = new Thread(RunAsyncCore<T>);
            this.messenger.IsBackground = true;
            this.messenger.SetApartmentState(ApartmentState.STA);
            this.messenger.Start(action);
        }

        private void RunAsyncCore<T>(object state)
        {
            try
            {
                object[] states = state as object[];
                Action<T> action = states[0] as Action<T>;
                T @object = (T)states[1];
                if (action != null)
                {
                    action(@object);
                    Application.Run(this);
                }
            }
            catch (ThreadInterruptedException) { }
            catch (ThreadAbortException) { }
        }

        protected sealed override void Dispose(bool disposing)
        {
            if (disposing && !closed)
            {
                try
                {
                    Exit();
                }
                catch
                { }
            }
            base.Dispose(disposing);
        }

        public void Exit()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null);
            if (this.closed)
                return;
            this.closed = true;
            ExitThread();
        }
    }
}
