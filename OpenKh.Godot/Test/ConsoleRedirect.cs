using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace OpenKh.Godot.Test;

public class ConsoleRedirect : TextWriter
{
    public static ConcurrentStack<string> MsgStack = new();
    public override Encoding Encoding => Encoding.Default;
    public override void WriteLine(string value) => MsgStack.Push(value);
    public override void WriteLine(uint value) => MsgStack.Push(value.ToString());
    public override void WriteLine(char[] buffer, int index, int count) => MsgStack.Push(new string(buffer).Substring(index, count));
    public override void WriteLine(long value) => MsgStack.Push(value.ToString());
    public override void WriteLine(int value) => MsgStack.Push(value.ToString());
    public override void WriteLine(object value) => MsgStack.Push(value?.ToString());
    public override void WriteLine(double value) => MsgStack.Push(value.ToString());
    public override void WriteLine(decimal value) => MsgStack.Push(value.ToString());
    public override void WriteLine(ReadOnlySpan<char> buffer) => MsgStack.Push(new string(buffer));
    public override void WriteLine(char[] buffer)  => MsgStack.Push(new string(buffer));
    public override void WriteLine(char value) => MsgStack.Push(value.ToString());
    public override void WriteLine(bool value) => MsgStack.Push(value.ToString());
    public override void WriteLine(float value) => MsgStack.Push(value.ToString());
    public override void WriteLine(string format, object arg0, object arg1) => MsgStack.Push(string.Format(format, arg0, arg1));
    public override void WriteLine(string format, object arg0, object arg1, object arg2) => MsgStack.Push(string.Format(format, arg0, arg1, arg2));
    public override void WriteLine(ulong value) => MsgStack.Push(value.ToString());
    public override void WriteLine(StringBuilder value) => MsgStack.Push(value?.ToString());
    public override void WriteLine(string format, params object[] arg) => MsgStack.Push(string.Format(format, arg));
    public override void WriteLine(string format, object arg0) => MsgStack.Push(string.Format(format, arg0));
    public override void WriteLine() { }
}
