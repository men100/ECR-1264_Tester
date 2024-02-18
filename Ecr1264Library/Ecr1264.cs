using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;

namespace ECR_1264_Utility
{
    public class Ecr1264
    {
        private SerialPort port;
        byte[] delimiter = { 0x0D }; // デリミタとなる CR(0x0D)

        public void Open(UInt16 portNum)
        {
            port = new SerialPort("COM" + portNum.ToString(), 9600, Parity.None, 8, StopBits.One);
            port.Open();
        }

        public void Close()
        {
            port.Close();
        }

        /*
         * 共通コマンド
         */
        private void Wait()
        {
            while (!port.CtsHolding)
            {
                System.Threading.Thread.Sleep(1);
            }
        }
        private void Command(String command)
        {
            Wait();
            port.Write(command);
            port.Write(delimiter, 0, 1);
        }
        private void Command(String command, UInt16 arg)
        {
            Wait();
            port.Write(command + arg.ToString());
            port.Write(delimiter, 0, 1);
        }
        private void Command(String command, UInt16 arg1, UInt16 arg2)
        {
            Wait();
            port.Write(command + arg1.ToString() + "," + arg2.ToString());
            port.Write(delimiter, 0, 1);
        }

        /*
         * 制御コマンド
         * 
         * VramClear : VRAM クリアコマンド
         * VRAM の内容を全て "0" にクリアし、同時に全ての表示ポインタを (0, 0) で初期化する
         * 
         * VramSet : VRAM セットコマンド
         * VRAM の内容を全て "1" にセットし、同時に全ての表示ポインタを (0, 0) で初期化する
         * 
         * ResetPointer : ポインタホームコマンド
         * 文字表示およビット表示のポインタを (0, 0) で初期化する
         * 
         * ReverseCharacter : 文字リバースコマンド
         * このコマンド以降の文字入力コマンドは、全て反転表示となる
         * 一度送信された状態でこのコマンドが再送信されると、反転表示が中止される
         * 
         * CancelReverseCharacter : 文字リバースキャンセルコマンド
         * 文字リバースコマンドを解除する
         * 
         * SetRewriteTime : 書換え時間コマンド
         * 画面の書き換え時間をパラメータ設定する、大きいほど時間が長い、1 から 5 の範囲
         * 
         * EnterSleep : スリープインコマンド
         * スリープモードへ移行する
         * 
         * LeaveSleep : スリープアウトコマンド
         * スリープモードを解除する
         * 
         * PowerDown : パワーダウンコマンド
         * 省電力モードに移行する、このコマンド以後は RESET 信号を掛けるまで如何なるコマンドも受け付けなくなる
         */
        public void VramClear()
        {
            Command("ER");
        }
        public void VramSet()
        {
            Command("EW");
        }
        public void ResetPointer()
        {
            Command("HH");
        }    
        public void ReverseCharacter()
        {
            Command("RV");
        }
        public void CancelReverseCharacter()
        {
            Command("RC");
        }
        public void SetRewriteTime(UInt16 time)
        {
            if (time <= 0 || 5 < time)
            {
                return;
            }
            Command("DT", time);
        }
        public void EnterSleep()
        {
            Command("SI");
        }
        public void LeaveSleep()
        {
            Command("SO");
        }
        public void PowerDown()
        {
            Command("PD");
        }
        public void UpdateDisplay()
        {
            Command("DP");
        }
        public void UpdateDisplayPartial(UInt16 startBlock, UInt16 blockNum)
        {
            if (7 < startBlock || blockNum == 0 || 8 < blockNum)
            {
                return;
            }
            if (8 < startBlock + blockNum)
            {
                return;
            }
            Command("PS", startBlock, blockNum);
        }

        /*
         * 文字 Base
         * 
         * WriteStringBase
         * MoveStringPointerBase
         */
        private void WriteStringBase(string command, string str)
        {
            const int border = 64; // 参考 : バッファ 96byte, 96*(2/3) = 64byte
            int index = 0;
            int restSize = str.Length;
            
            // 製品仕様上一行に収めるべしとあるので、やっぱりうまくいかない
            // あるサイズからおかしくなってくる（複数行表示自体はできる）
            while (restSize >= border) 
            {
                Wait();
                port.Write(command + "'" + str.Substring(index, border) + "'");
                port.Write(delimiter, 0, 1);
                index += border;
                restSize -= border;
            }
            if (restSize > 0)
            {
                Wait();
                port.Write(command + "'" + str.Substring(index, restSize) + "'");
                port.Write(delimiter, 0, 1);
            }
        }
        private void MoveStringPointerBase(string command, UInt16 x, UInt16 y, UInt16 limitX, UInt16 limitY)
        {
            if (limitX < x || limitY < y)
            {
                return;
            }
            Command(command, x, y);
        }
        private string GetJisString(string str)
        {
            string jisStr = "";
            byte[] data = Encoding.GetEncoding("iso-2022-jp").GetBytes(str); // JIS

            // 何故か先頭と後半 3byte にはゴミが付くので無視する
            // BitConverter で変換すると '-' がつくので即座に string配列にバラす
            // そしてそれをまた string に戻す(無駄だ・・・何とかならんかしら)
            string[] strArray = BitConverter.ToString(data, 3, str.Length * 2).Split('-');
            for (int i = 0; i < strArray.Length; i++)
            {
                jisStr += strArray[i];
            }

            return jisStr;
        }

        /*
         * 半角文字(ASCII 文字 で 8x16 ドット)
         * 
         * WriteAscii : 半角文字入力コマンド
         * 半角文字を描画する、描画した文字の分だけ半角文字ポインタが移動する
         * 
         * MoveAsciiPointer : 半角文字ポインタ移動コマンド
         * 半角文字の入力座標を設定する、 (0,0) から (15,3) の範囲
         * 
         * LineFeedAscii : 半角文字ラインフィードコマンド
         * 現在の半角文字ポインタの Y 座標に 1 加算する、3 のときは 0 になる
         * 
         * CarriageReturnAscii : 半角文字キャリッジリターンコマンド
         * 現在の半角文字ポインタの X 座標を 0 にする
         */
        public void WriteAscii(string str)
        {
            WriteStringBase("HW", str);
        }
        public void MoveAsciiPointer(UInt16 x, UInt16 y)
        {
            MoveStringPointerBase("HP", x, y, 15, 3);
        }
        public void LineFeedAscii()
        {
            Command("HF");
        }
        public void CarriageReturnAscii()
        {
            Command("HR");
        }

        /*
         * ANK 文字(ASCII 文字 で 8x8 ドット)
         * 
         * WriteAnk : ANK 文字入力コマンド
         * ANK 文字を描画する、描画した文字の分だけ ANK 文字ポインタが移動する
         * 
         * MoveAnkPointer : ANK 文字ポインタ移動コマンド
         * ANK 文字の入力座標を設定する、 (0,0) から (15,7) の範囲
         * 
         * LineFeedAnk : ANK 文字ラインフィードコマンド
         * 現在の ANK 文字ポインタの Y 座標に 1 加算する、7 のときは 0 になる
         * 
         * CarriageReturnAnk : ANK 文字キャリッジリターンコマンド
         * 現在の ANK 文字ポインタの X 座標を 0 にする
         */
        public void WriteAnk(string str)
        {
            WriteStringBase("CW", str);
        }
        public void MoveAnkPointer(UInt16 x, UInt16 y)
        {
            MoveStringPointerBase("CP", x, y, 15, 7);
        }
        public void LineFeedAnk()
        {
            Command("CF");
        }
        public void CarriageReturnAnk()
        {
            Command("CR");
        }

        /*
         * 半角小文字(ASCII 文字 で 6x16 ドット)
         * 
         * WriteSmallAscii : 半角小文字入力コマンド
         * 半角小文字を描画する、描画した文字の分だけ半角小文字ポインタが移動する
         * 
         * MoveSmallAsciiPointer : 半角小文字ポインタ移動コマンド
         * 半角小文字の入力座標を設定する、 (0,0) から (20,3) の範囲
         * 
         * LineFeedSmallAscii : 半角小文字ラインフィードコマンド
         * 現在の半角小文字ポインタの Y 座標に 1 加算する、3 のときは 0 になる
         * 
         * CarriageReturnSmallAscii : 半角小文字キャリッジリターンコマンド
         * 現在の半角小文字ポインタの X 座標を 0 にする
         */
        public void WriteSmallAscii(string str)
        {
            WriteStringBase("AW", str);
        }
        public void MoveSmallAsciiPointer(UInt16 x, UInt16 y)
        {
            MoveStringPointerBase("AP", x, y, 20, 3);
        }
        public void LineFeedSmallAscii()
        {
            Command("AF");
        }
        public void CarriageReturnSmallAscii()
        {
            Command("AR");
        }

        /*
         * 全角漢字 (JIS 第一&第二水準漢字で 16x16 ドット)
         * 
         * WriteJis : 全角漢字入力コマンド
         * 全角漢字を描画する、描画した文字の分だけ全角漢字ポインタが移動する
         * 
         * MoveJisPointer : 全角漢字ポインタ移動コマンド
         * 全角漢字の入力座標を設定する、 (0,0) から (7,3) の範囲
         * 
         * LineFeedJis : 全角漢字ラインフィードコマンド
         * 現在の全角漢字ポインタの Y 座標に 1 加算する、3 のときは 0 になる
         * 
         * CarriageReturnJis : 全角漢字キャリッジリターンコマンド
         * 現在の全角漢字ポインタの X 座標を 0 にする
         */
        public void WriteJis(string str)
        {
            WriteStringBase("KW", GetJisString(str));
        }
        public void MoveJisPointer(UInt16 x, UInt16 y)
        {
            MoveStringPointerBase("KP", x, y, 7, 3);
        }
        public void LineFeedJis()
        {
            Command("KF");
        }
        public void CarriageReturnJis()
        {
            Command("KR");
        }

        /*
         * 全角小漢字 (JIS 第一&第二水準漢字で 12x16 ドット)
         * 
         * WriteSmallJis : 全角小漢字入力コマンド
         * 全角小漢字を描画する、描画した文字の分だけ全角小漢字ポインタが移動する
         * 
         * MoveSmallJisPointer : 全角小漢字ポインタ移動コマンド
         * 全角小漢字の入力座標を設定する、 (0,0) から (9,3) の範囲
         * 
         * LineFeedSmallJis : 全角小漢字ラインフィードコマンド
         * 現在の全角小漢字ポインタの Y 座標に 1 加算する、3 のときは 0 になる
         * 
         * CarriageReturnSmallJis : 全角小漢字キャリッジリターンコマンド
         * 現在の全角小漢字ポインタの X 座標を 0 にする
         */
        public void WriteSmallJis(string str)
        {
            WriteStringBase("SW", GetJisString(str));
        }
        public void MoveSmallJisPointer(UInt16 x, UInt16 y)
        {
            MoveStringPointerBase("SP", x, y, 9, 3);
        }
        public void LineFeedSmallJis()
        {
            Command("SF");
        }
        public void CarriageReturnSmallJis()
        {
            Command("SR");
        }

        /*
         * 拡張
         * 
         * 改行付き
         * WriteLineAscii
         * WriteLineAnk
         * WriteLineSmallAscii
         * WriteLineJis
         * WriteLineSmallJis
         */
        public void WriteLineAscii(string str)
        {
            WriteStringBase("HW", str);
            LineFeedAscii();
            CarriageReturnAscii();
        }
        public void WriteLineAnk(string str)
        {
            WriteStringBase("CW", str);
            LineFeedAnk();
            CarriageReturnAnk();
        }
        public void WriteLineSmallAscii(string str)
        {
            WriteStringBase("AW", str);
            LineFeedSmallAscii();
            CarriageReturnSmallAscii();
        }
        public void WriteLineJis(string str)
        {
            WriteStringBase("KW", GetJisString(str));
            LineFeedJis();
            CarriageReturnJis();
        }
        public void WriteLineSmallJis(string str)
        {
            WriteStringBase("SW", GetJisString(str));
            LineFeedSmallJis();
            CarriageReturnSmallJis();
        }

        /*
         * ビット表示
         * 
         * WriteBitPattern : ビット表示コマンド
         * 
         * MoveBitPatternPointer : ビット表示ポインタ移動コマンド
         */
        public void WriteBitPattern(string str)
        {
            const Int16 BIT_PATTERN_LENGTH = 16; // 0x[][] * 8 = 16
            string pattern;

            if (str.Length > BIT_PATTERN_LENGTH)
            {
                pattern = str.Substring(0, BIT_PATTERN_LENGTH);
            }
            else if (str.Length < BIT_PATTERN_LENGTH)
            {
                pattern = str;
                pattern.PadRight(BIT_PATTERN_LENGTH);
            }
            else
            {
                pattern = str;
            }

            // このコマンドのみデリミタの必要なし
            Wait();
            port.Write("GW");
            port.Write(BitConverter.GetBytes(Convert.ToInt64(pattern, 16)), 0, 8);
        }
        public void WriteBitPattern(byte[] data)
        {
            const Int16 BIT_PATTERN_SIZE = 8;

            Wait();
            port.Write("GW");
            if (data.Length >= BIT_PATTERN_SIZE)
            {
                port.Write(data, 0, BIT_PATTERN_SIZE);
            }
            else
            {
                byte[] padding = new byte[BIT_PATTERN_SIZE - data.Length];
                port.Write(data, 0, data.Length);
                port.Write(padding, 0, BIT_PATTERN_SIZE - data.Length);
            }
        }

        public void MoveBitPatternPointer(UInt16 x, UInt16 y)
        {
            MoveStringPointerBase("GP", x, y, 120, 7);
        }

        /*
         * 画面
         */
        public void LoadScreenData(string fileName)
        {
            const UInt16 SCREEN_DATA_SIZE = 1024;

            if (!File.Exists(fileName))
            {
                return;
            }

            FileStream reader = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[SCREEN_DATA_SIZE]; // ゼロで初期化されると思って良いらしい

            if (reader.Length >= SCREEN_DATA_SIZE)
            {
                reader.Read(data, 0, SCREEN_DATA_SIZE);
            }
            else
            {               
                reader.Read(data, 0, (int)reader.Length);
            }
            reader.Close();

            Wait();
            port.Write("TI");
            port.Write(data, 0, SCREEN_DATA_SIZE);
            port.Write(delimiter, 0, 1);
        }

    }
}
