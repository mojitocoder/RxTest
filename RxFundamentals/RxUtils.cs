using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxFundamentals
{
    public static class RxUtils
    {
        public static IObservable<byte[]> AsyncRead(this Stream stream, int bufferSize)
        {
            var asyncRead = Observable.FromAsyncPattern<byte[], int, int, int>(stream.BeginRead, stream.EndRead);

            var buffer = new byte[bufferSize];

            return asyncRead(buffer, 0, bufferSize).Select(readBytes =>
            {
                var newBuffer = new byte[readBytes];
                Array.Copy(buffer, newBuffer, readBytes);
                return newBuffer;
            });
        }

        public static IObservable<string> AsyncReadLines(this Stream stream, int bufferSize)
        {
            return Observable.Create<string>(observer =>
            {
                var sb = new StringBuilder();
                var blocks = AsyncRead(stream, bufferSize).Select(block => Encoding.ASCII.GetString(block));

                Action produceCurrentLine = () =>
                {
                    var text = sb.ToString();
                    sb.Length = 0;
                    observer.OnNext(text);
                };

                return blocks.Subscribe(data =>
                {
                    for (var i = 0; i < data.Length; i++)
                    {
                        var atEndofLine = false;
                        var c = data[i];

                        if (c == '\r')
                        {
                            atEndofLine = true;
                            var j = i + 1;
                            if (j < data.Length && data[j] == '\n')
                                i++;
                        }
                        else if (c == '\n')
                        {
                            atEndofLine = true;
                        }

                        if (atEndofLine)
                        {
                            produceCurrentLine();
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                },
                observer.OnError,
                () =>
                {
                    produceCurrentLine();
                    observer.OnCompleted();
                });
            });
        }
    }
}
