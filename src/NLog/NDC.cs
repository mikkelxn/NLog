// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections;
using System.Text;

namespace NLog
{
    // stack implemented as an ArrayList
    public sealed class NDC
    {
        private NDC() { }

        public static IDisposable Push(string text) {
            ArrayList stack = GetThreadStack();
            int previousCount = stack.Count;
            stack.Add(text);
            return new StackPopper(stack, previousCount);
        }

        public static string Pop() {
            ArrayList stack = GetThreadStack();
            if (stack.Count > 0) {
                string retVal = (string)stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                return retVal;
            } else {
                return String.Empty;
            }
        }

        public static string GetTopMessage() {
            ArrayList stack = GetThreadStack();
            if (stack.Count > 0) {
                return (string)stack[stack.Count - 1];
            } else {
                return String.Empty;
            }
        }

        public static void Clear() {
            ArrayList stack = GetThreadStack();

            stack.Clear();
        }

        public static string GetAllMessages(string separator) {
            ArrayList stack = GetThreadStack();
            if (stack.Count == 0)
                return String.Empty;

            if (stack.Count == 1)
                return GetTopMessage();

            int totalLength = ((string)stack[0]).Length;
            for (int i = 1; i < stack.Count; ++i) {
                totalLength += separator.Length;
            }
            StringBuilder sb = new StringBuilder(totalLength);
            sb.Append((string)stack[0]);
            for (int i = 1; i < stack.Count; ++i) {
                sb.Append(separator);
                sb.Append((string)stack[i]);
            }
            return sb.ToString();
        }

        public static string GetBottomMessages(int count, string separator) {
            ArrayList stack = GetThreadStack();
            if (count > stack.Count)
                count = stack.Count;
            if (count == 0)
                return String.Empty;

            if (count == 1)
                return GetTopMessage();

            int totalLength = ((string)stack[0]).Length;
            for (int i = 1; i < count; ++i) {
                totalLength += separator.Length;
            }
            StringBuilder sb = new StringBuilder(totalLength);
            sb.Append((string)stack[0]);
            for (int i = 1; i < count; ++i) {
                sb.Append(separator);
                sb.Append((string)stack[i]);
            }
            return sb.ToString();
        }

        public static string GetTopMessages(int count, string separator) {
            ArrayList stack = GetThreadStack();
            if (count >= stack.Count)
                return GetAllMessages(separator);

            int pos0 = stack.Count - 1 - count;
            int totalLength = ((string)stack[pos0]).Length;
            for (int i = pos0 + 1; i < stack.Count; ++i) {
                totalLength += separator.Length;
            }
            StringBuilder sb = new StringBuilder(totalLength);
            sb.Append((string)stack[pos0]);
            for (int i = pos0 + 1; i < stack.Count; ++i) {
                sb.Append(separator);
                sb.Append((string)stack[i]);
            }
            return sb.ToString();
        }

        private static ArrayList GetThreadStack()
        {
            if (_threadStack == null)
                _threadStack = new ArrayList();

            return _threadStack;
        }

        private class StackPopper : IDisposable
        {
            private ArrayList _stack;
            private int _previousCount;

            public StackPopper(ArrayList stack, int previousCount) {
                _stack = stack;
                _previousCount = previousCount;
            }

            void IDisposable.Dispose()
            {
                while (_stack.Count > _previousCount) {
                    _stack.RemoveAt(_stack.Count - 1);
                }
            }
        }

        [ThreadStatic]
        private static ArrayList _threadStack;
    }
}