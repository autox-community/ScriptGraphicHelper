using Avalonia;
using ScriptGraphicHelper.Models;
using ScriptGraphicHelper.Views;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

public class Dmsoft
{
    private Type? Obj = null;
    private object? Obj_object = null;
    private static Dmsoft? instance = null;
    public static Dmsoft Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Dmsoft();
            }
            return instance;
        }
    }

    public int Hwnd { get; set; } = -1;
    public string Display { get; set; } = string.Empty;
    public string Mouse { get; set; } = "normal";
    public string Keypad { get; set; } = "normal";
    public string Public_desc { get; set; } = string.Empty;
    public int Mode { get; set; } = 0;


    public int? BindWindowEx()
    {
        return BindWindowEx(this.Hwnd, this.Display, this.Mouse, this.Keypad, this.Public_desc, this.Mode);
    }

    public Point GetClientSize()
    {
        GetClientSize(this.Hwnd, out var w, out var h);
        return new Point(w, h);
    }

    public byte[] GetScreenData(int w, int h)
    {
        var intPtr = (IntPtr)GetScreenData(0, 0, w, h);
        var len = w * h * 4;
        var data = new byte[len];
        Marshal.Copy(intPtr, data, 0, len);
        return data;
    }

    public bool Reg()
    {
        var regCode = Settings.Instance.DmRegcode;
        if (regCode == string.Empty || regCode == "")
        {
            MessageBox.ShowAsync("错误, 需要在setting.json文件中填写大漠注册码");
            return false;
        }

        var result = Reg(regCode, "") ?? -999;
        if (result == 1)
        {
            return true;
        }
        else
        {
            MessageBox.ShowAsync("注册大漠失败, 返回值: " + result.ToString());
            return false;
        }
    }


    public Dmsoft()
    {
        try
        {
            this.Obj = Type.GetTypeFromProgID("dm.dmsoft");
            this.Obj_object = Activator.CreateInstance(this.Obj);
            Reg();
        }
        catch (Exception e)
        {
            MessageBox.ShowAsync(e.Message + ", 无法在系统中找到大漠插件!");
        }

    }

    // 调用此接口进行com对象释放
    public void ReleaseObj()
    {
        if (this.Obj_object != null)
        {
            Marshal.ReleaseComObject(this.Obj_object);
            this.Obj_object = null;
        }
    }

    ~Dmsoft()
    {
        ReleaseObj();
    }

    public string? Ver()
    {
        object result;
        result = this.Obj.InvokeMember("Ver", BindingFlags.InvokeMethod, null, this.Obj_object, null);
        return result.ToString() ?? string.Empty;
    }


    public int? Reg(string code, string Ver)
    {
        var args = new object[2];
        object result;
        args[0] = code;
        args[1] = Ver;

        result = this.Obj.InvokeMember("Reg", BindingFlags.InvokeMethod, null, this.Obj_object, args);
        return (int?)result;
    }

    public int? UnBindWindow()
    {
        object result;
        result = this.Obj.InvokeMember("UnBindWindow", BindingFlags.InvokeMethod, null, this.Obj_object, null);
        return (int?)result;
    }

    public int? GetClientSize(int hwnd, out int width, out int height)
    {
        var args = new object[3];
        object result;
        var mods = new ParameterModifier[1];

        mods[0] = new ParameterModifier(3);
        mods[0][1] = true;
        mods[0][2] = true;

        args[0] = hwnd;

        result = this.Obj.InvokeMember("GetClientSize", BindingFlags.InvokeMethod, null, this.Obj_object, args, mods, null, null);
        width = (int)args[1];
        height = (int)args[2];
        return (int?)result;
    }

    public int? GetScreenData(int x1, int y1, int x2, int y2)
    {
        var args = new object[4];
        object result;
        args[0] = x1;
        args[1] = y1;
        args[2] = x2;
        args[3] = y2;

        result = this.Obj.InvokeMember("GetScreenData", BindingFlags.InvokeMethod, null, this.Obj_object, args);
        return (int?)result;
    }

    public string? EnumWindow(int parent, string title, string class_name, int filter)
    {
        var args = new object[4];
        object result;
        args[0] = parent;
        args[1] = title;
        args[2] = class_name;
        args[3] = filter;

        result = this.Obj.InvokeMember("EnumWindow", BindingFlags.InvokeMethod, null, this.Obj_object, args);
        return result.ToString();
    }


    public string? GetWindowTitle(int hwnd)
    {
        var args = new object[1];
        object result;
        args[0] = hwnd;

        result = this.Obj.InvokeMember("GetWindowTitle", BindingFlags.InvokeMethod, null, this.Obj_object, args);
        return result.ToString();
    }


    public int? GetMousePointWindow()
    {
        object result;
        result = this.Obj.InvokeMember("GetMousePointWindow", BindingFlags.InvokeMethod, null, this.Obj_object, null);
        return (int?)result;
    }


    public int? GetWindow(int hwnd, int flag)
    {
        var args = new object[2];
        object result;
        args[0] = hwnd;
        args[1] = flag;

        result = this.Obj.InvokeMember("GetWindow", BindingFlags.InvokeMethod, null, this.Obj_object, args);
        return (int?)result;
    }

    public string? GetWindowClass(int hwnd)
    {
        var args = new object[1];
        object result;
        args[0] = hwnd;

        result = this.Obj.InvokeMember("GetWindowClass", BindingFlags.InvokeMethod, null, this.Obj_object, args);
        return result.ToString();
    }


    public int? BindWindowEx(int hwnd, string display, string mouse, string keypad, string public_desc, int mode)
    {
        var args = new object[6];
        object result;
        args[0] = hwnd;
        args[1] = display;
        args[2] = mouse;
        args[3] = keypad;
        args[4] = public_desc;
        args[5] = mode;

        result = this.Obj.InvokeMember("BindWindowEx", BindingFlags.InvokeMethod, null, this.Obj_object, args);
        return (int?)result;
    }
}