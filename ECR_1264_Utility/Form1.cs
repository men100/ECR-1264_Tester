using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;

namespace ECR_1264_Utility
{
    public partial class Form1 : Form
    {
        const Double VERSION = 1.00;

        public Ecr1264 ecr;

        public Form1()
        {
            InitializeComponent();
            ecr = new Ecr1264();
        }

        private bool ecrWrapperOpen(string message)
        {
            try
            {
                ecr.Open(UInt16.Parse(ComPortBox.Text));
            }
            catch
            {
                logLabel.Text = "ERROR : コマンド \"" + message + "\" の発行に失敗しました。"; 
                return false;
            }
            return true;
        }

        private void ecrWrapperClose(string message)
        {
            try
            {
                ecr.Close();
            }
            catch
            {
                // do nothing
            }
            logLabel.Text = "SUCCESS : コマンド \"" + message + "\" の発行に成功しました。"; 
        }

        // 制御系
        private void erButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("ER"))
            {
                ecr.VramClear();
                ecrWrapperClose("ER");
            }
        }
        private void ewButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("EW"))
            {
                ecr.VramSet();
                ecrWrapperClose("EW");
            }
        }
        private void hhButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("HH"))
            {
                ecr.ResetPointer();
                ecrWrapperClose("HH");
            }
        }
        private void rvButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("RV"))
            {
                ecr.ReverseCharacter();
                ecrWrapperClose("RV");
            }
        }
        private void rcButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("RC"))
            {
                ecr.CancelReverseCharacter();
                ecrWrapperClose("RC");
            }
        }
        private void dtButton_Click(object sender, EventArgs e)
        {
            if (dtParameterBox.Text != "")
            {
                if (ecrWrapperOpen("DT"))
                {
                    ecr.SetRewriteTime(UInt16.Parse(dtParameterBox.Text));
                    ecrWrapperClose("DT");
                }
            }
        }
        private void siButton_Click(object sender, EventArgs e)
        {
            if (siCheckBox.Checked)
            {
                if (ecrWrapperOpen("SI"))
                {
                    ecr.EnterSleep();
                    ecrWrapperClose("SI");
                }
            }
            else
            {
                MessageBox.Show("実行許可にチェックして下さい");
            }
        }
        private void soButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("SO"))
            {
                ecr.LeaveSleep();
                ecrWrapperClose("SO");
            }
        }
        private void pdButton_Click(object sender, EventArgs e)
        {
            if (pdCheckBox.Checked)
            {
                if (ecrWrapperOpen("PD"))
                {
                    ecr.PowerDown();
                    ecrWrapperClose("PD");
                }
            }
            else
            {
                MessageBox.Show("実行許可にチェックして下さい");
            }
        }

        // 表示書き換え系
        private void dpButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("DP"))
            {
                ecr.UpdateDisplay();
                ecrWrapperClose("DP");
            }
        }
        private void psButton_Click(object sender, EventArgs e)
        {
            if (psStartBlockBox.Text != "" && psBlockWidthBox.Text != "")
            {
                if (ecrWrapperOpen("PS"))
                {
                    ecr.UpdateDisplayPartial(
                        UInt16.Parse(psStartBlockBox.Text), UInt16.Parse(psBlockWidthBox.Text));
                    ecrWrapperClose("PS");
                }
            }
        }

        private void バージョン情報VToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Ver : " + String.Format("{0:f2}", VERSION) + "\n" + "Author : 綿100%",
                            "ECR-1264 テスター");
        }

        // 全角漢字
        private void kwButton_Click(object sender, EventArgs e)
        {
            if (kwTextBox.Text != "")
            {
                if (ecrWrapperOpen("KW"))
                {
                    ecr.WriteJis(kwTextBox.Text);
                    ecrWrapperClose("KW");
                }
            }
        }
        private void kpButton_Click(object sender, EventArgs e)
        {
            if (kpXBox.Text != "" && kpYBox.Text != "")
            {
                if (ecrWrapperOpen("KP"))
                {
                    ecr.MoveJisPointer(UInt16.Parse(kpXBox.Text), UInt16.Parse(kpYBox.Text));
                    ecrWrapperClose("KP");
                }
            }
        }
        private void kfButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("KF"))
            {
                ecr.LineFeedJis();
                ecrWrapperClose("KF");
            }
        }
        private void krButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("KR"))
            {
                ecr.CarriageReturnJis();
                ecrWrapperClose("KR");
            }
        }

        // 半角文字
        private void hwButton_Click(object sender, EventArgs e)
        {
            if (hwTextBox.Text != "")
            {
                if (ecrWrapperOpen("HW"))
                {
                    ecr.WriteAscii(hwTextBox.Text);
                    ecrWrapperClose("HW");
                }
            }
        }
        private void hpButton_Click(object sender, EventArgs e)
        {
            if (hpXBox.Text != "" && hpYBox.Text != "")
            {
                if (ecrWrapperOpen("HP"))
                {
                    ecr.MoveAsciiPointer(UInt16.Parse(hpXBox.Text), UInt16.Parse(hpYBox.Text));
                    ecrWrapperClose("HP");
                }
            }
        }
        private void hfButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("HF"))
            {
                ecr.LineFeedAscii();
                ecrWrapperClose("HF");
            }
        }
        private void hrButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("HR"))
            {
                ecr.CarriageReturnAscii();
                ecrWrapperClose("HR");
            }
        }

        // ANK 文字
        private void cwButton_Click(object sender, EventArgs e)
        {
            if (cwTextBox.Text != "")
            {
                if (ecrWrapperOpen("CW"))
                {
                    ecr.WriteAnk(cwTextBox.Text);
                    ecrWrapperClose("CW");
                }
            }
        }
        private void cpButton_Click(object sender, EventArgs e)
        {
            if (cpXBox.Text != "" && cpYBox.Text != "")
            {
                if (ecrWrapperOpen("CP"))
                {
                    ecr.MoveAnkPointer(UInt16.Parse(cpXBox.Text), UInt16.Parse(cpYBox.Text));
                    ecrWrapperClose("CP");
                }
            }
        }
        private void cfButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("CF"))
            {
                ecr.LineFeedAnk();
                ecrWrapperClose("CF");
            }
        }
        private void crButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("CR"))
            {
                ecr.CarriageReturnAnk();
                ecrWrapperClose("CR");
            }
        }

        // 全角小漢字
        private void swButton_Click(object sender, EventArgs e)
        {
            if (swTextBox.Text != "")
            {
                if (ecrWrapperOpen("SW"))
                {
                    ecr.WriteSmallJis(swTextBox.Text);
                    ecrWrapperClose("CW");
                }
            }
        }
        private void spButton_Click(object sender, EventArgs e)
        {
            if (spXBox.Text != "" && spYBox.Text != "")
            {
                if (ecrWrapperOpen("SP"))
                {
                    ecr.MoveSmallJisPointer(UInt16.Parse(spXBox.Text), UInt16.Parse(spYBox.Text));
                    ecrWrapperClose("SP");
                }
            }
        }
        private void sfButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("SF"))
            {
                ecr.LineFeedSmallJis();
                ecrWrapperClose("SF");
            }
        }
        private void srButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("SR"))
            {
                ecr.CarriageReturnSmallJis();
                ecrWrapperClose("SR");
            }
        }

        // 半角小文字
        private void awButton_Click(object sender, EventArgs e)
        {
            if (awTextBox.Text != "")
            {
                if (ecrWrapperOpen("AW"))
                {
                    ecr.WriteSmallAscii(awTextBox.Text);
                    ecrWrapperClose("AW");
                }
            }
        }
        private void apButton_Click(object sender, EventArgs e)
        {
            if (apXBox.Text != "" && apYBox.Text != "")
            {
                if (ecrWrapperOpen("AP"))
                {
                    ecr.MoveSmallAsciiPointer(UInt16.Parse(apXBox.Text), UInt16.Parse(apYBox.Text));
                    ecrWrapperClose("AP");
                }
            }
        }
        private void afButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("AF"))
            {
                ecr.LineFeedSmallAscii();
                ecrWrapperClose("AF");
            }
        }
        private void arButton_Click(object sender, EventArgs e)
        {
            if (ecrWrapperOpen("AR"))
            {
                ecr.CarriageReturnSmallAscii();
                ecrWrapperClose("AR");
            }
        }

        private void gwButton_Click(object sender, EventArgs e)
        {
            if (gwTextBoxLeft.Text != "" && gwTextBoxRight.Text != "")
            {
                if (ecrWrapperOpen("GW"))
                {
                    ecr.WriteBitPattern(gwTextBoxLeft.Text + gwTextBoxRight.Text);
                    ecrWrapperClose("GW");
                }
            }
        }

        private void gpButton_Click(object sender, EventArgs e)
        {
            if (gpXBox.Text != "" && gpYBox.Text != "")
            {
                if (ecrWrapperOpen("GP"))
                {
                    ecr.MoveBitPatternPointer(UInt16.Parse(gpXBox.Text), UInt16.Parse(gpYBox.Text));
                    ecrWrapperClose("GP");
                }
            }
        }

        private void tiFileButton_Click(object sender, EventArgs e)
        {
            openTiFileDialog.Filter = "BIN ファイル (*.bin)|*.bin|すべてのファイル (*.*)|*.*";
            if (openTiFileDialog.ShowDialog() == DialogResult.OK)
            {
                tiLabel.Text = openTiFileDialog.FileName;
            }
        }

        private void tiButton_Click(object sender, EventArgs e)
        {
            if (tiLabel.Text != "")
            {
                if (ecrWrapperOpen("TI"))
                {
                    ecr.LoadScreenData(tiLabel.Text);
                    ecrWrapperClose("TI");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
