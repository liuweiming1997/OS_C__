using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace WindowsFormsApp1 {
	public partial class Form1 : Form {
		static int signal = 0;//0:empty 1:read 2:write
		static int numberReader = 0, numberWriter = 0;
		private static object obj = new object();
		private static object si = new object();
		private static object reader = new object();
		const int dis = 1000;
		static Stopwatch timer1;
		static int needReader = 0;
		class temp {
			public string[] str;
			int getSignal() {
				lock (si) {
					return signal;
				}
			}
			int getNumber() {
				if (0 == getSignal()) return 0;
				else if (1 == getSignal()) return numberReader;
				else return numberWriter;
			}
			void addSignal(int id) {
				lock (si) {
					if (id == 1) {
						signal = 1;
						numberReader++;
					} else {
						signal = 2;
						numberWriter++;
					}
				}
			}
			void cutSignal(int id) {
				lock (si) {
					if (id == 1) {
						numberReader--;
						if (numberReader == 0) signal = 0;
					} else {
						numberWriter--;
						if (numberWriter == 0) signal = 0;
					}
				}
			}

			void addStringRichText(String str) {
				lock (obj) {
					Form1.richTextBox1.Text += str;
					Form1.richTextBox1.Text += '\n';
					Form1.richTextBox1.Focus();
					Form1.richTextBox1.Select(Form1.richTextBox1.Text.Length, 0);
					Form1.richTextBox1.ScrollToCaret();

				}
			}

			void show(String[] str, int id) {
				Console.Write(id + "+++ ");
				for (int i = 0; i < str.Length; ++i) {
					Console.Write(str[i] + " ");
				}
				Console.WriteLine("\n");
			}

			void addReader(int op) {
				lock (reader) {
					needReader += op;
				}
			}

			int getNeedReader() {
				lock (reader) {
					return needReader;
				}
			}

			void semWait(int id) {
				if (id == 1) {
					addReader(1); // 首先需要申请需要资源，直接资源锁定
					Thread.Sleep(50);
					if (getSignal() != 2) {
						addSignal(id);
						return;
					}
					while (getSignal() == 2) { } // 合法后立马addSignal
					addSignal(1);
				} else {
					addReader(0);
					Thread.Sleep(50); // good
					while (getNeedReader() > 0) { }
					while (getSignal() != 0) { } //这句后面肯定要是addSignal，立马的
					addSignal(2);
				}
			}

			void semSignal(int id) {
				if (id == 1) {
					cutSignal(1);
					addReader(-1);
				} else {
					cutSignal(2);
				}
			}

			public void Reader() {
				addStringRichText("读者：" + str[0] + " 线程创建");
				Thread.Sleep(int.Parse(str[2]) * 1000);
				addStringRichText("读者：" + str[0] + " 在时间：" + (int)timer1.Elapsed.TotalSeconds + " 请求读操作");

				/*进行读操作*/
				semWait(1);
				addStringRichText("读者：" + str[0] + " 在时间：" + (int)timer1.Elapsed.TotalSeconds + " 开始读操作");
				Thread.Sleep(int.Parse(str[3]) * 1000);
				addStringRichText("读者：" + str[0] + " 在时间：" + (int)timer1.Elapsed.TotalSeconds + " 结束读操作");
				semSignal(1);
				/*读操作进行完毕*/



			}
			private static object w = new object();
			public void Writer() {
				addStringRichText("写者：" + str[0] + " 线程创建");
				Thread.Sleep(int.Parse(str[2]) * 1000);
				addStringRichText("写者：" + str[0] + " 在时间：" + (int)timer1.Elapsed.TotalSeconds + " 请求写操作");

				semWait(2);
				/*进行读操作*/

				addStringRichText("写者：" + str[0] + " 在时间：" + (int)timer1.Elapsed.TotalSeconds + " 开始写操作");
				Thread.Sleep(int.Parse(str[3]) * 1000);
				addStringRichText("写者：" + str[0] + " 在时间：" + (int)timer1.Elapsed.TotalSeconds + " 结束写操作");
				semSignal(2);
				/*读操作进行完毕*/

			}

		}
		public Form1() {
			InitializeComponent();
		}
		private void button1_Click(object sender, EventArgs e) {
			OpenFileDialog op = new OpenFileDialog();
			op.ShowDialog();
			op.Filter = "All files（*.*）|*.*";
			StreamReader reader = null;

			try {
				string fileName = op.FileName;
				if (fileName == "") {
					return;
				}
				richTextBox1.Clear();
				reader = new StreamReader(fileName, System.Text.Encoding.Default);
				string str = reader.ReadLine();
				List<Thread> list = new List<Thread>();
				while (str != null) {
					string[] res = str.Split(' ');
					temp zhu = new temp();
					zhu.str = res;
					if (str.Contains("R") || str.Contains("r")) {
						Thread rThread = new Thread(zhu.Reader);
						//rThread.Priority = ThreadPriority.AboveNormal;
						rThread.Priority = ThreadPriority.AboveNormal;
						list.Add(rThread);

					} else if (str.Contains("W") || str.Contains("w")) {
						Thread wThread = new Thread(zhu.Writer);
						//wThread.Priority = ThreadPriority.BelowNormal;
						list.Add(wThread);
					} else {
						MessageBox.Show("输入数据有错误！！！【每行数据后面不能有空格】，【文件结束不能有空行】");
						return;
					}
					//MessageBox.Show(str);
					str = reader.ReadLine();
				}
				timer1 = new Stopwatch();//计时器类
				timer1.Start();
				foreach (Thread i in list) {
					i.Start();
				}
			} catch (Exception error) {
				MessageBox.Show("错误信息是：" + error.Message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);

			} finally {
				if (reader != null) {
					reader.Close();
				}
			}
			const string MutexName = "Multithreading";
			var m = new Mutex(false, MutexName);


		}
		private void richTextBox1_TextChanged(object sender, EventArgs e) {
		}

		private void button2_Click(object sender, EventArgs e) {

		}

		private void Form1_Load(object sender, EventArgs e) {
			System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false; //允许其他线程修改
		}
	}
}
