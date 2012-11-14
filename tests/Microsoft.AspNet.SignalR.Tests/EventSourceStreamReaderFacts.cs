﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Transports.ServerSentEvents;
using Xunit;

namespace Microsoft.AspNet.SignalR.Tests
{
    public class EventSourceStreamReaderFacts : IDisposable
    {
        [Fact]
        public void ReadTriggersOpenedOnOpen()
        {
            var memoryStream = MemoryStream("data:somedata\n\n");
            var wh = new ManualResetEvent(false);
            var tcs = new TaskCompletionSource<string>();
            var eventSource = new EventSourceStreamReader(memoryStream);

            eventSource.Opened = () => wh.Set();
            eventSource.Message = sseEvent => tcs.TrySetResult(sseEvent.Data);

            eventSource.Start();
            Assert.True(wh.WaitOne(TimeSpan.FromSeconds(5)));
            Assert.True(tcs.Task.Wait(TimeSpan.FromSeconds(5)));
            Assert.Equal("somedata", tcs.Task.Result);
        }

        [Fact]
        public void CloseThrowsSouldntTakeProcessDown()
        {
            var memoryStream = MemoryStream("");
            var eventSource = new EventSourceStreamReader(memoryStream);

            eventSource.Closed = (ex) =>
            {
                throw new Exception("Throw on closed");
            };

            eventSource.Start();
                        
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Force any finalizers to run so we can see unhandled task errors
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private MemoryStream MemoryStream(string data)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(data));
        }
    }
}
