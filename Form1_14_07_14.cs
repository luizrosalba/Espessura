using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace Espessura
{
     
    
      

    public partial class Form1 : Form
    {
        int opcao_programa=0; /// nada escolhido 
        int opcao_metodologia = 0;                      /// 
        string nome_arquivo;
        List <double> energia = new List<Double>();
        List<double>  contagem = new List<Double>();

        /// <summary>
        ///  armazenam so a regiao do kalpha e kbeta 
        /// </summary>

        List<double> kalpha_energia = new List<Double>();
        List<double> kalpha_contagem = new List<Double>();

        List<double> kbeta_energia = new List<Double>();
        List<double> kbeta_contagem = new List<Double>();

        /// <summary>
        ///  armazenam so a regiao do kalpha e kbeta interpolados 
        /// </summary>

        List<double> kalpha_interp_energia = new List<Double>();
        List<double> kalpha_interp_cont = new List<Double>();

        List<double> kbeta_interp_energia = new List<Double>();
        List<double> kbeta_interp_cont = new List<Double>();


    


        public Form1()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        
            InitializeComponent();
            
            energia.Clear();
        
            contagem.Clear();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            
            
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void arquivoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void abrirArquivoToolStripMenuItem_Click(object sender, EventArgs e)
        {




        }

        private void AnaliseEspectro(object sender, EventArgs e)
        {
            /// executar o gnuplot , plotar o grafico e salvar e abrir no programa 
            /// 
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboBox1.SelectedIndex == 0) /// Metodologia 1 selecionada 
            {
                
                pictureBox1.Load("C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\metodologia1.jpg");
                opcao_metodologia = 1;
                Painel_Met_5.Visible = true; 


            }
            if (comboBox1.SelectedIndex == 1)/// Metodologia 2 selecionada 
            {
                pictureBox1.Load("C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\metodologia2.jpg");
                opcao_metodologia = 2;
            }
            if (comboBox1.SelectedIndex == 2)/// Metodologia 3 selecionada 
            {
                pictureBox1.Load("C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\metodologia3.jpg");
                opcao_metodologia = 3;
            }
            if (comboBox1.SelectedIndex == 3)/// Metodologia 4 selecionada 
            {
                pictureBox1.Load("C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\metodologia4.jpg");
                opcao_metodologia = 4;
            }
            if (comboBox1.SelectedIndex == 4)/// Metodologia 4 selecionada 
            {
                pictureBox1.Load("C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\metodologia5.jpg");
                opcao_metodologia = 5;
                Painel_Met_5.Visible = true; 
            }

        }


        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBoxCalib_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// gera numpontos pontos entre _liminf e _limsup , armazena no vetor saida 
        /// </summary>
        /// <param name="_liminf"></param>
        /// <param name="_limsup"></param>
        /// <param name="numpontos"></param>
        /// <param name="saida"></param>
        void Gera_Intervalo ( double _liminf,double _limsup , double  numpontos , List <double > saida )
        {
            double temp=_liminf;
            double acrescimo = ((_limsup-_liminf)/numpontos); 
            do
            {
                saida.Add(temp);
                temp += acrescimo;
            } while (temp < _limsup);
        }

        void Escreve_arquivo(List<double> list1, List<double> list2, string nome)
        {
            //...
            string separator = "\t \t";
            using (StreamWriter writer = new StreamWriter(nome))
            {
                for (int i = 0; i < Math.Max(list1.Count, list2.Count); i++)
                {
                    var element1 = i < list1.Count ? list1[i].ToString() : "";
                    var element2 = i < list2.Count ? list2[i].ToString() : "";
                    writer.Write(element1);
                    writer.Write(separator);
                    writer.WriteLine(element2);
                }
            }
        }

        /// <summary>
        
        /// </summary>
        /// <param name="_x"> x é o eixo x dos dados de entrada   </param>
        /// <param name="_y"> y é o eixo y dos dados de entrada </param>
        /// 
        /// <param name="gerado"> é o vetor com numpontos dados do eixo x gerados  </param>
        /// <param name="saida">  é o vetor com numpontos dados do eixo y interpolados </param>
        /// gera numpontos dados entre x menor e x maior e substitui na função interpolada  
        /// 

        private void Interp_Lagrange(List<double> _x, List<double> _y, string nome_arq_saida, List<double> _saidaenergia, List<double> _saidacontagem)
        {
            int numpontos;
            double menor, maior;
            
            /// pegando o menor e o maior valor no vetor x de entrada 
            menor = _x.Min();
            maior = _x.Max();

            /// tratar para nao receber null 
            numpontos = Convert.ToInt16(textBox15.Text);

            /// gerando n pontos igualmente espaçados e armazena no vetor _saidaenergia 
            /// 

            Gera_Intervalo(menor,maior,numpontos,_saidaenergia);

            int tamdados = _saidaenergia.Count();
           
            int tamx = _x.Count();


            //MessageBox.Show("Inicio Lagrange");

	        // calculo dos polinomios L
	        double somatorio=0.0;
	        double prod=1.0;

	        for (int k=0;k<tamdados;k++)
	        {
		        somatorio=0.0;
		        for (int i=0; i<tamx;i++)
		        {
			        prod=1.0;
			        //calculo do Lj
			        for (int j=0;j<tamx;j++)
			        {
                        if ( j != i )
				        {
                            prod *= (_saidaenergia.ElementAt(k) - _x.ElementAt(j)) / (_x.ElementAt(i) - _x.ElementAt(j));
                            
				        }
			        }
                    //System.Console.WriteLine(i + " " + prod); 
			        somatorio += _y.ElementAt(i)*prod;
                    
		        }
                _saidacontagem.Add(somatorio);
	        }

            //MessageBox.Show("Fim LAgrange");

            /// escrever em um arquivo a interpolação feita 
            /// 
            Escreve_arquivo(_saidaenergia, _saidacontagem, nome_arq_saida); 
 
        }



        private void dPCMAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            opcao_programa = 1; /// DPCMA selecionado 
            Stream myStream = null;
            string line;                        /// string para guardar as linhas enquanto sao lidas 
            
            List<Int64> canal = new List<Int64>(); /// vetor que guardara as contagens em cada canal

            int index;


            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\tempo 600 segundos";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            /// vou tentar fazer tudo em c#... 
                            /// MessageBox.Show(openFileDialog1.FileName); 
                            //string text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                            nome_arquivo = openFileDialog1.FileName;
                            System.IO.StreamReader file = new System.IO.StreamReader(nome_arquivo);
                            textBoxCalib.Clear();
                            while ((line = file.ReadLine()) != null)
                            {

                                if (line == "<<PMCA SPECTRUM>>")
                                {
                                    line = file.ReadLine();
                                    while (line != "<<CALIBRATION>>")
                                    {
                                        index = line.IndexOf("GAIN");
                                        if (index != -1)
                                        {
                                            textBoxganho.Text = line.Substring(7, (line.Length - 7));/// a partir do caractere 5 pega até o fim  caracteres /// pegando o numero de canais
                                        }

                                        index = line.IndexOf("START_TIME - ");
                                        if (index != -1)
                                        {
                                            textBoxhora.Text = line.Substring(13, (line.Length - 13));/// a partir do caractere 5 pega até o fim  caracteres /// pegando o numero de canais
                                        }

                                        line = file.ReadLine();

                                    }


                                }



                                if (line == "<<CALIBRATION>>")
                                {
                                    line = file.ReadLine();
                                    while (line != "<<DATA>>")
                                    {
                                        textBoxCalib.AppendText(line);
                                        textBoxCalib.AppendText("\n");

                                        line = file.ReadLine();
                                    }
                                }

                                if (line == "<<DATA>>")
                                {
                                    line = file.ReadLine();
                                    while (line != "<<END>>")
                                    {
                                        //textBoxCalib.AppendText(line);
                                        //textBoxCalib.AppendText("\n");
                                        canal.Add(Convert.ToInt64(line));
                                        line = file.ReadLine();
                                    }
                                }
                                //Console.WriteLine(canal[i]);
                                //i++;





                                if (line == "<<DP5 CONFIGURATION>>")
                                {
                                    line = file.ReadLine();
                                    while (line != "<<DPP STATUS END>>")
                                    {

                                        if (line.Substring(0, 5) == "MCAC=") /// pegando o numero de canais
                                        {
                                            index = line.IndexOf(';');
                                            txtboxcanais.Text = line.Substring(5, index - 5);/// a partir do caractere 5 pega 4 caracteres
                                        }

                                        if (line.Substring(0, 5) == "PRER=")
                                        {
                                            index = line.IndexOf(';');
                                            txtboxtempo.Text = line.Substring(5, index - 5);/// a par
                                        }/// pegando o numero de canais

                                        //textBoxCalib.AppendText(line);
                                        //textBoxCalib.AppendText("\n");

                                        line = file.ReadLine();
                                    }
                                }
                            }
                            //textBoxCalib.Text=text;


                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                }
            }

            /// passando a linha do numero de contagens no canal desejado.
            /// na proxima versão , vou usar os dados da calibracao para 
            /// buscar em qual canal estará o K alpha desejado 
            /// 



            if (textBoxI0.Text != null)
            {

                textBoxI0.Text = canal[1020].ToString(); /// contagens do canal do k aplha do ferro 6,4 keV canal = 1020
                textBoxIs.Text = canal[1371].ToString(); /// contagens do canal do k aplha da cobertura por
                /// enquanto zinco com ferro 10  8,615 canal = 1371
                /// 
                /// passando os coef. de absorção a energia de 15 keV (a usada no raio x)
                /// mesmo aviso que o usado para os canais , mas tarde isso devera ser 
                /// automatico
                double temp;
                temp = 57.2;
                textBoxmuE0.Text = temp.ToString();  /// coef. abs. ferro a 15 keV 
                temp = 80.8;
                textBoxmuKAlpha.Text = temp.ToString();  /// coef. abs. zinco a 15 keV 


                /// calculando a espessura pela metodologia 1 
                /// 
                double Is, Ic, theta, espE0, espKalpha;
                Ic = Convert.ToDouble(textBoxI0.Text);
                Is = Convert.ToDouble(textBoxIs.Text);
               // theta = Convert.ToDouble(textBoxtheta.Text);
                espE0 = Convert.ToDouble(textBoxmuE0.Text) * 7.13;
                espKalpha = Convert.ToDouble(textBoxmuKAlpha.Text) * 7.13;
                //temp = Math.Log((Is / Ic), 2) * Math.Cos(theta * Math.PI / 180) / (espE0 + espKalpha);
                textBoxespessura.Text = temp.ToString();
            }

        }
        
       

        private void geant4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            opcao_programa = 2; /// Geant4 selecionado 

            Stream myStream = null;
            string line;                        /// string para guardar as linhas enquanto sao lidas 
            //string nome_arquivo;                /// nome do arquido lido 
            List<double> canal = new List<double>(); /// vetor que guardara as contagens em cada canal

            int index;

            /// Abrindo o arquivo 
            ///  ORGANIZAR ! 

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\tempo 600 segundos";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        Arquivo_aberto.Clear();
                        using (myStream)
                        {
                            
                            /// MessageBox.Show(openFileDialog1.FileName); 

                            nome_arquivo = openFileDialog1.FileName;

                            string text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                            Arquivo_aberto.AppendText(nome_arquivo);
                            Arquivo_aberto.AppendText(System.Environment.NewLine);
                            Arquivo_aberto.AppendText(text);

                            string[] lines = System.IO.File.ReadAllLines(openFileDialog1.FileName);
                            int i = 0;

                            energia.Clear();
                            contagem.Clear();

                            ///abrindo o arquivo em duas colunas e convertendo para double 
                            foreach (string linha in lines)
                            {
                                string[] separado = linha.Split(new Char[] { ' ', '\t' });

                                foreach (string s in separado)
                                {

                                    if (s.Trim() != "")
                                    {
                                        if (i == 0)
                                        {
                                            //Console.WriteLine(Convert.ToDouble(s, System.Globalization.CultureInfo.InvariantCulture));
                                            energia.Add(Convert.ToDouble(s, System.Globalization.CultureInfo.InvariantCulture));
                                            
                                            i = 1;
                                        }
                                        else
                                        {
                                            //Console.WriteLine(Convert.ToDouble(s, System.Globalization.CultureInfo.InvariantCulture));
                                            contagem.Add(Convert.ToDouble(s, System.Globalization.CultureInfo.InvariantCulture));
                                            i = 0;
                                        }
                                    }
                                        
                                }
                                
                                
                               
                            }


                        }
                    }
                    //AnaliseEspectro();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                }
            }
        }
        

        private void mCGolo64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            opcao_programa = 3; /// MCgolo  selecionado 
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Painel_Met_5_VisibleChanged(object sender, EventArgs e)
        {
            /// se existir o arquivo de dados anterior  , carregar 
        }

        private void label25_Click(object sender, EventArgs e)
        {

        }

        void Escreve_Comandos_Gnuplot()
        { 
            /// cria o arquivo que fornecerá os comandos para o gnuplot 
            /// 

            string temp , barra , barras  ;
            barra = "\\";
            barras = "\\\\";

            temp = "reset;";
            temp += "clear;";
            temp += "set term jpeg;";
            temp += "set xtics "+ textBox17.Text +";";

            temp += "set xrange [" + textBox10.Text  + ":" + textBox16.Text + "];";


            temp += "set yrange [" + textBox27.Text + ":" + textBox26.Text + "];";
            temp += "set ytics " + textBox25.Text + ";";
            
            temp += "set output \"saida.jpg\" ;";
            //temp += "plot \"a.out\" w i  title 'Discrete counts';"; /// funciona 
            temp += "plot \"";
            temp += nome_arquivo.Replace(barra, barras);
            temp += "\" w l title 'Dados', ";

            if (checkBox1.Checked == true) /// plotar também a interpolacao 
            {
                string caminho = "\"C:\\\\Users\\\\max\\\\Google Drive\\\\LuizRosalba pc na uerj\\\\Espessura\\\\Espessura\\\\bin\\\\Release\\\\interpolacaokalpha.dat";

                //temp += "replot \"";
                temp += caminho;
                temp += "\" w p title 'kalpha_interp', ";

                caminho = "\"C:\\\\Users\\\\max\\\\Google Drive\\\\LuizRosalba pc na uerj\\\\Espessura\\\\Espessura\\\\bin\\\\Release\\\\interpolacaokbeta.dat";

                //temp += "replot \"";
                temp += caminho;
                temp += "\" w p title 'kbeta_interp';";

                
            }
            
            
            //textBox1.Text = nome_arquivo.Replace(barra,barras);
            //temp += nome_arquivo.Replace(barra, barras); 
            //temp += "'a.out'" ; 
            //temp += " \" w i  title 'Discrete counts';";
            File.WriteAllText("comandos.out",temp);

            
        
        }



        void Interpola(int algoritmo) /// com interpolacao 
        {


            kalpha_interp_energia.Clear();
            kalpha_interp_cont.Clear();
            kbeta_interp_energia.Clear();
            kbeta_interp_cont.Clear();

            switch (comboBox2.SelectedIndex)
            {
                case 0: /// Lagrange 
                        /// 
                        ///  interpola e guarda a interpolacao de um intervalo definido no form nos vetores de energia e contagem interpolados 
                        Interp_Lagrange(kalpha_energia, kalpha_contagem, "interpolacaokalpha.dat",kalpha_interp_energia , kalpha_interp_cont); /// interpola a regiao do k alpha 
                        Interp_Lagrange(kbeta_energia, kbeta_contagem, "interpolacaokbeta.dat",kbeta_interp_energia , kbeta_interp_cont);   //// interpola a regiao do k beta 
                    break;
                case 1: /// Gaussiana 
                    
                    break;
                case 2: /// Newton
                    
                    break;
                case 3:  /// Splines Cúbicos 
                    break;
                case -1:  /// Erro 
                    MessageBox.Show("Escolha um Algoritmo de interpolação ");
                    break;
            
            }
            
            



        }
        void  ChamaGnuplot()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\Espessura\\bin\\Release\\bin\\gnuplot.exe";
            startInfo.Arguments += " -e ";
            startInfo.Arguments += " \" load \'comandos.out \' \" ";
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.UseShellExecute = true;
            Process p;
            p = Process.Start(startInfo);
            p.WaitForExit();
        }

        private void Calcula_regiao_Ka_Kb()
        {
             /// 
            /// cria dois subvetores de energia e contagem para interpolacao do pico do kaplha e k beta 
            ///  os subvetores kalpha e kbeta interpolados já estao armazenados quando fazemos a interpolacao 

            kalpha_contagem.Clear();
            kalpha_energia.Clear();

            kbeta_interp_cont.Clear();
            kbeta_contagem.Clear();

            int posmin, posmax, quantidade;

            /// Encontra a posicao da energia desejada no array energia e copia para um sub array kalpha_energia e k_alpha contagem 
            posmin = EncontraPosicao(Convert.ToDouble(textBox6.Text, System.Globalization.CultureInfo.InvariantCulture), energia);
            posmax = EncontraPosicao(Convert.ToDouble(textBox22.Text, System.Globalization.CultureInfo.InvariantCulture), energia);
            quantidade = posmax - posmin;

            /// pega uma subparte do vetor energia e guarda para calcular o kalpha interpolado 
            kalpha_energia = energia.GetRange(posmin, quantidade);
            kalpha_contagem = contagem.GetRange(posmin, quantidade);

            /// Encontra a posicao da energia desejada no array energia e copia para um sub array kbeta_energia e k_beta contagem 
            posmin = EncontraPosicao(Convert.ToDouble(textBox23.Text, System.Globalization.CultureInfo.InvariantCulture), energia);
            posmax = EncontraPosicao(Convert.ToDouble(textBox24.Text, System.Globalization.CultureInfo.InvariantCulture), energia);
            quantidade = posmax - posmin;

            kbeta_energia = energia.GetRange(posmin, quantidade);
            kbeta_contagem = contagem.GetRange(posmin, quantidade);

           


        }
        private void button2_Click(object sender, EventArgs e)
        {
           
            /// chamar o gnuplot para plotar o grafico da funcao 
            /// ajustar 
            /// 
            //
            textBox28.Text = nome_arquivo;

            /// se houver arquivo aberto 
            if (contagem.Count > 1)
            {
                if (checkBox1.Checked==true )   /// caixa da interpolacao marcada , com interpolacao 
                {


                    if (comboBox2.SelectedIndex != (-1)) // se escolheu algum método de interpolacao válido 
                    {
                        Calcula_regiao_Ka_Kb();
                        Interpola(comboBox2.SelectedIndex);
                        Escreve_Comandos_Gnuplot();

                        
                        ChamaGnuplot();
                        pictureBox2.LoadAsync("C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\Espessura\\bin\\Release\\saida.jpg");
                    }
                    else
                    {
                        MessageBox.Show("Escolha um método de interpolação !   ");
                    }
                        

                    
                }
                else /// sem interpolacao 
                {
                    Calcula_regiao_Ka_Kb();
                    Escreve_Comandos_Gnuplot();
                    ChamaGnuplot();
                    pictureBox2.LoadAsync("C:\\Users\\max\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\Espessura\\bin\\Release\\saida.jpg");
                }

                
               
                
            }
            else
            {
                MessageBox.Show("Erro: Carregue um arquivo de dados para leitura !  " );
            
            }
            
            

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        static int Compare_Pelo_Valor_2(KeyValuePair<double, double> a, KeyValuePair<double, double> b)
        {
            return a.Value.CompareTo(b.Value);
        }

        static int Compare_Pela_Contagem(KeyValuePair<KeyValuePair<double, double>, int> a, KeyValuePair<KeyValuePair<double, double>, int> b)
        {
            return a.Key.Value.CompareTo(b.Key.Value);
        }


        /// <summary>
        /// Dentro de um vetor , procura o primeiro valor a esquerda
        /// Que é menor que um valor buscado. Retorna a posicao dentro do vetor que atende
        /// esta condicao 
        /// </summary>
        /// <param name="pos_inic"></param>
        /// <param name="valor_buscado"></param>
        /// <param name="_ent"></param>
        /// <returns></returns>
        private double Procure_a_esquerda(int pos_inic , double valor_buscado , List<double> _ent)
        {
            for (int i = pos_inic; i >= 0; i--)
            {
                if (_ent.ElementAt(i) <= valor_buscado)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// Dentro de um vetor , procura o primeiro valor a direita
        /// que é menor que um valor buscado. Retorna a posicao dentro do vetor que atende
        /// esta condicao 
        /// </summary>
        /// <param name="pos_inic"></param>
        /// <param name="valor_buscado"></param>
        /// <param name="_ent"></param>
        /// <returns></returns>

        private double Procure_a_direita(int pos_inic, double valor_buscado, List<double> _ent)
        {
            for (int i = pos_inic; i < _ent.Count; i++)
            {
                if (_ent.ElementAt(i) <= valor_buscado)
                    return i;
            }
            return -1;
        }

        /// <summary>
        ///  retorna as energias   onde teremos a contagem 
        ///  com valor igual a metade das contagens do pico   
        /// </summary>
        /// <param name="contagens"></param>
        /// <returns></returns>
        private void Calcula_Largura_Meia_Altura (int pos_inic,  double _contagens , double en_min , double en_max)
        {
            double metade_contagens = _contagens / 2.0;

            /// dentro do vetor energia buscar as energias da largura a meia altura 
            
            /// Procura o primeiro valor menor que a metade das contagens a esquerda 
            ///
            en_min=Procure_a_esquerda( pos_inic , metade_contagens , energia);
            /// Procura o primeiro valor menor que a metade das contages a direita 
            /// 
            MessageBox.Show(" min " + en_min.ToString());
            en_max = Procure_a_direita(pos_inic, metade_contagens, energia);
            MessageBox.Show(" max " + en_max.ToString());

        }

        void DetectaPicos(int num_picos_manter , double energia_maxima)
        { 
            ///verificar se há grafico carregado , senão mostrar message box 
            ///
            /// percorrer o array buscando mudancas de concavidades 
            /// 

            double delta_antes , delta_depois, antes , meio , depois ;
           
            var energiaxcontagemxposen = new List<KeyValuePair<KeyValuePair<double, double>,int>>();
            // energiaxcontagemxposen.Key = par energia contagem
            // energiaxcontagemxposen.Key.Key = energia 
            // energiaxcontagemxposen.Key.Value = Contagem 
            // energiaxcontagemxposen.Value = Posicao no vetor contagem  

            int cont_pair = 0; 
          
            /// Algoritmo que busca picos 
            for (int i = 1; i < (contagem.Count-1); i++) /// do primeiro elemento ao penúltimo 
            {
                antes = Math.Abs(contagem.ElementAt(i - 1));
                meio = Math.Abs(contagem.ElementAt(i));
                depois = Math.Abs(contagem.ElementAt(i + 1));
                
                delta_antes = meio  - antes ;
                delta_depois = depois - meio;
                if (delta_antes > 0 && delta_depois < 0)
                { 
                    /// pico 
                    /// 
                   /// energia X contagem
                    var par = new KeyValuePair<double,double>(energia.ElementAt(i),contagem.ElementAt(i)); 
                    var par_par = new KeyValuePair<KeyValuePair<double,double>,int> (par,i); 
                    energiaxcontagemxposen.Add( par_par) ; 
                    /// posicao no vetor energia 
                    cont_pair++;
                }

                
            }

            /// fim algoritmo picos 
            /// 

            double en_prim_esq=0, /// energia do primeiro ponto com contagens menores
                                  /// que metade do pico a esquerda
                   en_prim_dir=0 ; /// energia do primeiro ponto com contagens menores
                                  /// que metade do pico a direita
            energiaxcontagemxposen.Sort(Compare_Pela_Contagem);
          
            energiaxcontagemxposen.Reverse();
          
            int cont =0 ; /// utilizado para saber quantos picos eu mantenho no meu richtextbox
            int posenergia = 0 ;   /// variavel temporaria 
            richTextBox1.Clear();  /// limpe o texto 

            foreach (var pair in energiaxcontagemxposen)  
            {
                if (cont < num_picos_manter && pair.Key.Key < energia_maxima) 
                {
                    richTextBox1.AppendText(pair.ToString() + "  " );                 
                    
                    /// posicao dentro do vetor energia 
                    posenergia = pair.Value;
                    
                    /// calcula a largura a meia altura do pico 
                       
                    Calcula_Largura_Meia_Altura(posenergia, pair.Key.Value,  en_prim_esq , en_prim_dir);
                    ///MessageBox.Show ( posenergia.ToString() + " " + pair.Key.Value + " " + en_prim_esq.ToString() + " " + en_prim_dir.ToString() );

                    // adicione as informacoes 
                    richTextBox1.AppendText( " LMA  =  " + en_prim_esq.ToString() + " " +en_prim_dir.ToString()  ) ;
                    richTextBox1.AppendText(System.Environment.NewLine);
                   
                    cont ++;
                }
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            
            Int16 num_manter = Convert.ToInt16(textBox1.Text);
            double energia_maxima = Convert.ToDouble(textBox4.Text);

            if (textBox1.Text.Length !=0)
            {
                DetectaPicos(num_manter , energia_maxima);
            }
            else
            {

                MessageBox.Show("Entre com o numero de picos a manter " , "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            


        }

        private void textBox1_TextChanged_2(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        ///  busca no vetor de entrada qual a posicao deste vetor que possui valor "valor"  
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private int EncontraPosicao(double valor , List <double> ent)
        { 
            int  i = 0 ;

            if (ent.Count == 0)
                return -1;



            while ( (i < ent.Count)  )
            {
                if (ent.ElementAt(i) == valor)
                    return i;
                else
                    i = i + 1;
            }
           

            if (i == (energia.Count-1))
                return -1; /// nao encontrou 
            else 
                return i; 

        }
        /// <summary>
        ///  Encontra no vetor a posicao do primeiro numero maior ou igual ao menor que estamos procunaod 
        /// le o vetor de frente para tras  
        ///  
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        private int Encontra_Maior_que (List<double> ent , double valor)
        {
            ///  percorrer os vetores até encontrar algum valor menor 
            ///  

            for (int i = 0; i < ent.Count; i++)
            {
                if (ent.ElementAt(i) >= valor)  return i;
            }

            return -1;
        }

        /// <summary>
        ///  encontra a posicao no vetor do primeior numero menor que o maior que estamos procurando 
        ///  le o vetor de tras para frente 
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        private int Encontra_Menor_que(List<double> ent, double valor)
        {
            for (int i = (ent.Count-1) ; i >= 0; i--)
            {
                if (ent.ElementAt(i) <= valor) return i;
            }

            return -1;
        }

        private void RecalculaRazao()
        { 
          
            /// melhorar  ERRADO !! percorrer o vetor e ir somando nada de utilizar esta  funcao encontra posicao !!!!!!!!
            if (checkBox2.Checked == false) /// usar dados sem interpolacao 
            {
                
                    double sumkalpha=0.0 , sumkbeta=0.0;
                    int posmenor = EncontraPosicao(Convert.ToDouble(textBox18.Text, System.Globalization.CultureInfo.InvariantCulture), energia );
                    int posmaior = EncontraPosicao(Convert.ToDouble(textBox19.Text, System.Globalization.CultureInfo.InvariantCulture) , energia);
                   
                    /// só receber valores que existam no vetor ou truncar para o próximo que exista 
                    for (int i = posmenor; i < posmaior; i++)
                    {
                        sumkalpha += contagem.ElementAt(i);

                    }

                    posmenor = EncontraPosicao(Convert.ToDouble(textBox20.Text, System.Globalization.CultureInfo.InvariantCulture) , energia );
                    posmaior = EncontraPosicao(Convert.ToDouble(textBox21.Text, System.Globalization.CultureInfo.InvariantCulture) , energia );

                    /// só receber valores que existam no vetor ou truncar para o próximo que exista 
                    for (int i = posmenor; i < posmaior; i++)
                    {
                        sumkbeta += contagem.ElementAt(i);
                    }

                    textBox3.Text = (sumkalpha / sumkbeta).ToString();  
            }



            if (checkBox2.Checked == true) /// usar dados interpolados 
            {

                int errokalpha, errokbeta = 0;
                double sumkalpha = 0.0, sumkbeta = 0.0;

                int posmenor = Encontra_Maior_que( kalpha_interp_energia, Convert.ToDouble(textBox18.Text, System.Globalization.CultureInfo.InvariantCulture));
                int posmaior = Encontra_Menor_que( kalpha_interp_energia , Convert.ToDouble(textBox19.Text, System.Globalization.CultureInfo.InvariantCulture));


                if ((posmaior == -1) || (posmenor == -1))
                {
                    MessageBox.Show("A posicao " + textBox18 + " ou a posicao " + textBox19 + "nao foram encontradas no vetor interpolado");
                    errokalpha = 1;
                }
                else
                {
                   // MessageBox.Show("Pos maior e menor kalpha " + posmaior.ToString() + " " + posmenor.ToString());
                    errokalpha = 0;
                    for (int i = posmenor; i < posmaior; i++)
                    {
                        sumkalpha += kalpha_interp_cont.ElementAt(i);

                    }
                }



                posmenor = Encontra_Maior_que(kbeta_interp_energia, Convert.ToDouble(textBox20.Text, System.Globalization.CultureInfo.InvariantCulture));
                posmaior = Encontra_Menor_que(kbeta_interp_energia , Convert.ToDouble(textBox21.Text, System.Globalization.CultureInfo.InvariantCulture) );
                /// só receber valores que existam no vetor ou truncar para o próximo que exista 


                if (posmaior == -1 || posmenor == -1)
                {
                    MessageBox.Show("A posicao " + textBox20.Text + " ou a posicao " + textBox21.Text + "nao foram encontradas no vetor interpolado");
                    errokbeta = 1;
                }
                else
                {
                    errokbeta = 0;
                    for (int i = posmenor; i < posmaior; i++)
                    {
                        sumkbeta += kbeta_interp_cont.ElementAt(i);
                    }

                }

                if (errokalpha == 0 && errokbeta == 0) textBox3.Text = (sumkalpha / sumkbeta).ToString();  


              
            }
            
        }
       

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            RecalculaRazao();
        }

        private void textBox18_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void textBox20_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox21_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox24_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox23_TextChanged(object sender, EventArgs e)
        {

        }

        
    }
}
