using MarcusW.VncClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyConnect.Protocols.Vnc
{
    public static class VncKeyMapping
    {
        /// <summary>
        /// Maps an Avalonia <see cref="Key"/> to a <see cref="KeySymbol"/>.
        /// </summary>
        /// <param name="key">The case Keys.</param>
        /// <param name="includePrintable">True, if printable chars should be included in the mapping, false otherwise.</param>
        /// <returns>The X key symbol.</returns>
        public static KeySymbol GetSymbolFromKey(Keys key, Keys modifiers, bool includePrintable = true)
        {
            KeySymbol keySymbol;

            switch (key)
            {
                case Keys.Cancel:
                    keySymbol = KeySymbol.Cancel;
                    break;

                case Keys.Back:
                    keySymbol = KeySymbol.BackSpace;
                    break;

                case Keys.Tab:
                    keySymbol = KeySymbol.Tab;
                    break;

                case Keys.LineFeed:
                    keySymbol = KeySymbol.Linefeed;
                    break;

                case Keys.Clear:
                    keySymbol = KeySymbol.Clear;
                    break;

                case Keys.Return:
                    keySymbol = KeySymbol.Return;
                    break;

                case Keys.Pause:
                    keySymbol = KeySymbol.Pause;
                    break;

                case Keys.CapsLock:
                    keySymbol = KeySymbol.Caps_Lock;
                    break;

                case Keys.Escape:
                    keySymbol = KeySymbol.Escape;
                    break;

                case Keys.Prior:
                    keySymbol = KeySymbol.Prior;
                    break;

                case Keys.PageDown:
                    keySymbol = KeySymbol.Page_Down;
                    break;

                case Keys.End:
                    keySymbol = KeySymbol.End;
                    break;

                case Keys.Home:
                    keySymbol = KeySymbol.Home;
                    break;

                case Keys.Left:
                    keySymbol = KeySymbol.Left;
                    break;

                case Keys.Up:
                    keySymbol = KeySymbol.Up;
                    break;

                case Keys.Right:
                    keySymbol = KeySymbol.Right;
                    break;

                case Keys.Down:
                    keySymbol = KeySymbol.Down;
                    break;

                case Keys.Select:
                    keySymbol = KeySymbol.Select;
                    break;

                case Keys.Print:
                    keySymbol = KeySymbol.Print;
                    break;

                case Keys.Execute:
                    keySymbol = KeySymbol.Execute;
                    break;

                case Keys.Insert:
                    keySymbol = KeySymbol.Insert;
                    break;

                case Keys.Delete:
                    keySymbol = KeySymbol.Delete;
                    break;

                case Keys.Help:
                    keySymbol = KeySymbol.Help;
                    break;

                case Keys.LWin:
                    keySymbol = KeySymbol.Super_L;
                    break;

                case Keys.RWin:
                    keySymbol = KeySymbol.Super_R;
                    break;

                case Keys.Apps:
                    keySymbol = KeySymbol.Menu;
                    break;

                case Keys.F1:
                    keySymbol = KeySymbol.F1;
                    break;

                case Keys.F2:
                    keySymbol = KeySymbol.F2;
                    break;

                case Keys.F3:
                    keySymbol = KeySymbol.F3;
                    break;

                case Keys.F4:
                    keySymbol = KeySymbol.F4;
                    break;

                case Keys.F5:
                    keySymbol = KeySymbol.F5;
                    break;

                case Keys.F6:
                    keySymbol = KeySymbol.F6;
                    break;

                case Keys.F7:
                    keySymbol = KeySymbol.F7;
                    break;

                case Keys.F8:
                    keySymbol = KeySymbol.F8;
                    break;

                case Keys.F9:
                    keySymbol = KeySymbol.F9;
                    break;

                case Keys.F10:
                    keySymbol = KeySymbol.F10;
                    break;

                case Keys.F11:
                    keySymbol = KeySymbol.F11;
                    break;

                case Keys.F12:
                    keySymbol = KeySymbol.F12;
                    break;

                case Keys.F13:
                    keySymbol = KeySymbol.F13;
                    break;

                case Keys.F14:
                    keySymbol = KeySymbol.F14;
                    break;

                case Keys.F15:
                    keySymbol = KeySymbol.F15;
                    break;

                case Keys.F16:
                    keySymbol = KeySymbol.F16;
                    break;

                case Keys.F17:
                    keySymbol = KeySymbol.F17;
                    break;

                case Keys.F18:
                    keySymbol = KeySymbol.F18;
                    break;

                case Keys.F19:
                    keySymbol = KeySymbol.F19;
                    break;

                case Keys.F20:
                    keySymbol = KeySymbol.F20;
                    break;

                case Keys.F21:
                    keySymbol = KeySymbol.F21;
                    break;

                case Keys.F22:
                    keySymbol = KeySymbol.F22;
                    break;

                case Keys.F23:
                    keySymbol = KeySymbol.F23;
                    break;

                case Keys.F24:
                    keySymbol = KeySymbol.F24;
                    break;

                case Keys.NumLock:
                    keySymbol = KeySymbol.Num_Lock;
                    break;

                case Keys.Scroll:
                    keySymbol = KeySymbol.Scroll_Lock;
                    break;

                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                    keySymbol = KeySymbol.Shift_L;
                    break;

                case Keys.RShiftKey:
                    keySymbol = KeySymbol.Shift_R;
                    break;

                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                    keySymbol = KeySymbol.Control_L;
                    break;

                case Keys.RControlKey:
                    keySymbol = KeySymbol.Control_R;
                    break;

                case Keys.Alt:
                    keySymbol = KeySymbol.Alt_L;
                    break;

                default:
                    keySymbol = KeySymbol.Null;
                    break;
            };

            if (keySymbol == KeySymbol.Null && includePrintable)
            {
                switch (key)
                {
                    case Keys.Space:
                        keySymbol = KeySymbol.space;
                        break;

                    case Keys.A:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.A : KeySymbol.a;
                        break;

                    case Keys.B:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.B : KeySymbol.b;
                        break;

                    case Keys.C:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.C : KeySymbol.c;
                        break;

                    case Keys.D:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.D : KeySymbol.d;
                        break;

                    case Keys.E:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.E : KeySymbol.e;
                        break;

                    case Keys.F:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.F : KeySymbol.f;
                        break;

                    case Keys.G:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.G : KeySymbol.g;
                        break;

                    case Keys.H:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.H : KeySymbol.h;
                        break;

                    case Keys.I:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.I : KeySymbol.i;
                        break;

                    case Keys.J:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.J : KeySymbol.j;
                        break;

                    case Keys.K:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.K : KeySymbol.k;
                        break;

                    case Keys.L:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.L : KeySymbol.l;
                        break;

                    case Keys.M:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.M : KeySymbol.m;
                        break;

                    case Keys.N:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.N : KeySymbol.n;
                        break;

                    case Keys.O:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.O : KeySymbol.o;
                        break;

                    case Keys.P:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.P : KeySymbol.p;
                        break;

                    case Keys.Q:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.Q : KeySymbol.q;
                        break;

                    case Keys.R:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.R : KeySymbol.r;
                        break;

                    case Keys.S:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.S : KeySymbol.s;
                        break;

                    case Keys.T:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.T : KeySymbol.t;
                        break;

                    case Keys.U:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.U : KeySymbol.u;
                        break;

                    case Keys.V:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.V : KeySymbol.v;
                        break;

                    case Keys.W:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.W : KeySymbol.w;
                        break;

                    case Keys.X:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.X : KeySymbol.x;
                        break;

                    case Keys.Y:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.Y : KeySymbol.y;
                        break;

                    case Keys.Z:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.Z : KeySymbol.z;
                        break;

                    case Keys.NumPad0:
                        keySymbol = KeySymbol.KP_0;
                        break;

                    case Keys.NumPad1:
                        keySymbol = KeySymbol.KP_1;
                        break;

                    case Keys.NumPad2:
                        keySymbol = KeySymbol.KP_2;
                        break;

                    case Keys.NumPad3:
                        keySymbol = KeySymbol.KP_3;
                        break;

                    case Keys.NumPad4:
                        keySymbol = KeySymbol.KP_4;
                        break;

                    case Keys.NumPad5:
                        keySymbol = KeySymbol.KP_5;
                        break;

                    case Keys.NumPad6:
                        keySymbol = KeySymbol.KP_6;
                        break;

                    case Keys.NumPad7:
                        keySymbol = KeySymbol.KP_7;
                        break;

                    case Keys.NumPad8:
                        keySymbol = KeySymbol.KP_8;
                        break;

                    case Keys.NumPad9:
                        keySymbol = KeySymbol.KP_9;
                        break;

                    case Keys.Multiply:
                        keySymbol = KeySymbol.KP_Multiply;
                        break;

                    case Keys.Add:
                        keySymbol = KeySymbol.KP_Add;
                        break;

                    case Keys.Subtract:
                        keySymbol = KeySymbol.KP_Subtract;
                        break;

                    case Keys.Decimal:
                        keySymbol = KeySymbol.KP_Decimal;
                        break;

                    case Keys.Divide:
                        keySymbol = KeySymbol.KP_Divide;
                        break;

                    case Keys.D1:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.exclam : KeySymbol.XK_1;
                        break;

                    case Keys.D2:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.at : KeySymbol.XK_2;
                        break;

                    case Keys.D3:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.numbersign : KeySymbol.XK_3;
                        break;

                    case Keys.D4:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.dollar : KeySymbol.XK_4;
                        break;

                    case Keys.D5:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.percent : KeySymbol.XK_5;
                        break;

                    case Keys.D6:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.asciicircum : KeySymbol.XK_6;
                        break;

                    case Keys.D7:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.ampersand : KeySymbol.XK_7;
                        break;

                    case Keys.D8:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.asterisk : KeySymbol.XK_8;
                        break;

                    case Keys.D9:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.parenleft : KeySymbol.XK_9;
                        break;

                    case Keys.D0:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.parenright : KeySymbol.XK_0;
                        break;

                    case Keys.OemMinus:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.underscore : KeySymbol.minus;
                        break;

                    case Keys.Oemplus:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.plus : KeySymbol.equal;
                        break;

                    case Keys.OemOpenBrackets:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.braceleft : KeySymbol.bracketleft;
                        break;

                    case Keys.OemCloseBrackets:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.braceright : KeySymbol.bracketright;
                        break;

                    case Keys.OemPipe:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.bar : KeySymbol.backslash;
                        break;

                    case Keys.Oemtilde:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.grave : KeySymbol.asciitilde;
                        break;

                    case Keys.OemSemicolon:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.colon : KeySymbol.semicolon;
                        break;

                    case Keys.OemQuotes:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.quotedbl : KeySymbol.quoteright;
                        break;

                    case Keys.Oemcomma:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.less : KeySymbol.comma;
                        break;

                    case Keys.OemPeriod:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.greater : KeySymbol.period;
                        break;

                    case Keys.OemQuestion:
                        keySymbol = (modifiers & Keys.Shift) == Keys.Shift ? KeySymbol.question : KeySymbol.slash;
                        break;

                    case Keys.Tab:
                        keySymbol = KeySymbol.Tab;
                        break;

                    default:
                        keySymbol = KeySymbol.Null;
                        break;
                };
            }

            return keySymbol;
        }

        /// <summary>
        /// Maps a printable char to a <see cref="KeySymbol"/>.
        /// </summary>
        /// <param name="c">The char.</param>
        /// <returns>The X key symbol.</returns>
        public static KeySymbol GetSymbolFromChar(char c)
        {
            if (c >= ' ' && c <= '~')
            {
                return KeySymbol.space + (c - ' ');
            }

            return (KeySymbol)(0x1000000 | c);
        }
    }
}
