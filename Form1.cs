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
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Numerics;
using Science;



namespace Espessura
{

    
        

    public partial class Form1 : Form
    {


        //string dir_inicial = "C:\\Users\\LR\\Google Drive\\artigo cmac 2015\\simulacoes cobre zerovirg644cm";
        string dir_inicial = "C:\\Users\\LR\\Google Drive\\medidasDOUTORADO\\placas frimafer\\medidas260615\\SDD\\raspagem15kv15microA\\Zinco";

        /// <summary>
        /// Variáveis publicas do programa 
        /// </summary>
        
        int opcao_programa = 0; /// nada escolhido 
        int opcao_metodologia = 0;                      /// 
        string nome_arquivo;
        int calcularazao = 0; // verifica se a razao foi calculada 

        double largurakalpha = 0.0;
        double largurakbeta = 0.0;
        int correcao_LMH_KA_KB = 0;

        double maxen = 0;
        double maxcont = 0;

        /// <summary>
        ///  posicao da energia minimas e maximas utilizadas na interpolacao 
        /// </summary>

        int pos_en_kalpha_min = 0;
        int pos_en_kalpha_max = 0;

        int pos_en_kbeta_min = 0;
        int pos_en_kbeta_max = 0;


        /// <summary>
        /// Lists do programa 
        /// </summary>
        /// 
            
            
            
            /// <summary>
            /// Energia e contagem lidas do arquivo 
            /// </summary>

            List<double> energia_original = new List<Double>();
            List<double> contagem_original = new List<Double>();

            /// <summary>
            /// Energia e contagem 
            /// </summary>

            List<double> energia = new List<Double>();
            List<double> contagem = new List<Double>();
        
            /// <summary>
            ///  Armazena contagem suavizada
            /// </summary>
            List<double> contagem_suave = new List<Double>();
            List<double> contagem_suave_sembg ;
            
            /// <summary>
            /// Armazena a interpolação cubica dos nodos 
            /// </summary>
            List<double> Interp_nodos = new List<Double>();

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

            /// <summary>
            /// armazena os nodos 
            /// </summary>
            List<int> nodo = new List<int>();

            /// <summary>
            ///  Armazena a interpolacao dos nodos  suavizada
            /// </summary>
           // List<double> Interp_nodos_suave = new List<Double>();
            


            /// <summary>
            ///  armazena os picos 
            /// </summary>
            ///  pico , valor do menor a esquerda , valor do menor a direita 
            List<KeyValuePair<double, KeyValuePair<double, double>>> picos = new List<KeyValuePair<double, KeyValuePair<double, double>>>();


            /// <summary>
            ///  Variaveis utilizadas para ler do DPPMCA 
            /// </summary>
            List<int> canal_calibracao = new List<int>();
            List<double> energia_calibracao = new List<double>();
            List<int> contagens_descalibradas = new List<int>();

        /// <summary>
        /// Fim Lists do programa 
        /// </summary>

            XrayLib xl = XrayLib.Instance; /// instancia do Xraylib 


            ///Dados para a Espessura 
            ///
            int Z_base;
            int Z_rev;
            int Z;
            string nome_elemento;
            double peso_molecular;
            double numatomico; 
            double energia_E0;
            double energiaKalphabase;
            double energiaKbetabase;
            double theta_entrada;
            double densidade;
            double peso_relativo;

        public Form1()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            this.AutoScrollPosition = new Point(0, 0);
            InitializeComponent();

            energia.Clear();

            contagem.Clear();

           
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            CarregaValores();

        }



        ////////////////////
        ///
        ///Algoritmos UTILS
        ///
        //////////////////////


        /// <summary>
        ///  minha funcao para converter de String para double com tratamento de ponto 
        /// </summary>
        /// <returns></returns>
        double StrtoDbl(string _ent)
        {
            if (_ent.CompareTo("") == 0)
            {
                MessageBox.Show("Faltam dados");
                return (-1.0);
            }

            else
                return (Convert.ToDouble(_ent, System.Globalization.CultureInfo.InvariantCulture));
        }


        /// <summary>
        ///  minha funcao para converter de String para int 32
        /// </summary>
        /// <returns></returns>
        Int32 StrtoInt(string _ent)
        {
            if (_ent.CompareTo("") == 0)
            {
                MessageBox.Show("Faltam dados");
                return (-1);
            }

            else
                return (Convert.ToInt32(_ent, System.Globalization.CultureInfo.InvariantCulture));
        }


        /// <summary>
        ///  busca no vetor de entrada qual a posicao deste vetor que possui valor "valor"  
        ///  Retorna -1 se não encontrado ou se vetor vazio 
        /// <param name="valor"></param>
        /// <returns></returns>
        private int EncontraPosicao(double valor, List<double> ent)
        {
            int i = 0;

            if (ent.Count == 0)
            {
                MessageBox.Show("Erro no Encontra Posica vetor vazio ");
                return -1;
            }


            while ((i < ent.Count))
            {
                if (Math.Abs(ent.ElementAt(i) - valor) < 0.01) // se proximo do valor 
                    return i;
                else
                    i = i + 1;
            }

            if (i == (energia.Count - 1))
            {
                MessageBox.Show("Erro no Encontra Posicao , a posicao buscada nao foi encontrada ");
                return -1; /// nao encontrou 
            }
            else
                return i;

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
        private int Procure_a_esquerda(int pos_inic, double valor_buscado, List<double> _ent)
        {
            for (int i = pos_inic; i >= 0; i--)
            {
                if (_ent.ElementAt(i) <= valor_buscado)
                {
                   // MessageBox.Show(_ent.ElementAt(i).ToString());
                    return i;
                }
                    
            }

            //MessageBox.Show("Problemas em encontrar o um valor menor que LMA a esquerda ");
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

        private int Procure_a_direita(int pos_inic, double valor_buscado, List<double> _ent)
        {
            for (int i = pos_inic; i < _ent.Count; i++)
            {
                if (_ent.ElementAt(i) <= valor_buscado)
                    return i;
            }
            //MessageBox.Show("Problemas em encontrar o um valor menor que LMA a esquerda ");
            return -1;
        }

        /// <summary>
        ///  Encontra no vetor a posicao do primeiro numero maior ou igual ao menor que estamos procurando 
        /// le o vetor de frente para tras  
        ///  
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        private int Encontra_Maior_que(List<double> ent, double valor)
        {

            for (int j = 0; j < ent.Count(); j++)
            {
               
                if (ent.ElementAt(j).CompareTo(valor) >= 0) return j;
            }

            return (-1);


        }



        /*
        ///  percorrer os vetores até encontrar algum valor menor 
        ///  
        int found;
        int index = ent.BinarySearch(valor);
        if (index < 0)  /// significa que buscara o primeiro maior 
        {
            found = ~(index) ;
            MessageBox.Show(" N Encontrei bitwise retorna o primeiro valor maior "     ); 

        }
        else  /// significa que encontrou exatamente o valor 
        {
            found = index;
            MessageBox.Show("Encontrei "); 

        }
            
        return found;
     */

        /// <summary>
        ///  encontra a posicao no vetor do primeior numero menor que o maior que estamos procurando 
        ///  le o vetor de tras para frente 
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        private int Encontra_Menor_que(List<double> ent, double valor)
        {
            int i = ent.Count;

            for (int j = (i - 1); j >= 0; j--)
            {

                if (ent.ElementAt(j).CompareTo(valor) <= 0) return j;
            }

            return (-1);

            /*

            int index = ent.BinarySearch(valor);
            int found;
            if (index < 0)  /// significa que index será o bitwise do primeiro maior valor que "valor"
            {

                found = (~index) - 1 ;   /// por isso volto uma posicao 

            }
            else // significa que encontrou exatamente o valor 
            {
                found = index;
                MessageBox.Show("Encontrei "); 

            }
           
            return found;
             * */
        }


        /// <summary>
        ///  retorna as energias   onde teremos a contagem 
        ///  com valor igual a metade das contagens do pico   
        /// </summary>
        /// <param name="contagens"></param>
        /// <returns></returns>
        private void Calcula_Largura_Meia_Altura(int pos_inic, double _contagens, out int pos_en_min, out int pos_en_max, List<double> _vec_contagem)
        {
            double metade_contagens = (_contagens / 2.0);

            /// dentro do vetor energia buscar as energias da largura a meia altura 

            /// Procura o primeiro valor menor que a metade das contagens a esquerda 
            ///
            
            pos_en_min = Procure_a_esquerda(pos_inic, metade_contagens, _vec_contagem);
           // MessageBox.Show("Esq " + pos_inic.ToString() + " " + metade_contagens.ToString() + " " + pos_en_min.ToString());
           
            /// Procura o primeiro valor menor que a metade das contages a direita 
            /// 
            //MessageBox.Show(" min " + en_min.ToString());
           // MessageBox.Show(pos_inic.ToString(), metade_contagens.ToString());
            pos_en_max = Procure_a_direita(pos_inic, metade_contagens, _vec_contagem);


            //MessageBox.Show("Dir " + pos_inic.ToString() + " " + metade_contagens.ToString() + " " + pos_en_max.ToString());
            //MessageBox.Show(" max " + en_max.ToString());

        }



        static int Compare_Pelo_Valor_2(KeyValuePair<double, double> a, KeyValuePair<double, double> b)
        {
            return a.Value.CompareTo(b.Value);
        }

        static int Compare_Pela_Contagem(KeyValuePair<KeyValuePair<double, double>, int> a, KeyValuePair<KeyValuePair<double, double>, int> b)
        {
            return a.Key.Value.CompareTo(b.Key.Value);
        }

        ////////////////////
        ///
        /// FIM Algoritmos UTILS
        ///
        //////////////////////


        private void arquivoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void abrirArquivoToolStripMenuItem_Click(object sender, EventArgs e)
        {




        }

       
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void SelecionaTab(int ind_tab)
        {
            
            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.Enabled = false;

            }

            (tabControl1.TabPages[ind_tab] as TabPage).Enabled = true;
            tabControl1.SelectedIndex = ind_tab;
            
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            tabControl1.Enabled = true; 

            if (comboBox1.SelectedIndex == 0) /// Metodologia 1 selecionada 
            {

                string path = Directory.GetCurrentDirectory();
                pictureBox1.Load(path + "\\metodologia1.jpg");
                opcao_metodologia = 1;
              
               // Painel_Met_5.Visible = true;
            }
            if (comboBox1.SelectedIndex == 1)/// Metodologia 2 selecionada 
            {
                string path = Directory.GetCurrentDirectory();
                
                pictureBox1.Load(path + "\\metodologia2.jpg");
                opcao_metodologia = 2;
              
            }

            
            if (comboBox1.SelectedIndex == 2)/// Metodologia 3 selecionada 
            {
                string path = Directory.GetCurrentDirectory();
                pictureBox1.Load(path + "\\metodologia3.jpg");
                opcao_metodologia = 3;
            }
            /*
            if (comboBox1.SelectedIndex == 3)/// Metodologia 4 selecionada 
            {
                string path = Directory.GetCurrentDirectory();
                pictureBox1.Load(path + "\\metodologia4.jpg");
                opcao_metodologia = 4;
            }
            if (comboBox1.SelectedIndex == 4)/// Metodologia 4 selecionada 
            {
                string path = Directory.GetCurrentDirectory();
                pictureBox1.Load(path + "\\metodologia5.jpg");
                opcao_metodologia = 5;
            }
            SelecionaTab(opcao_metodologia - 1);
            */


        }


        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
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
        void Gera_Intervalo(double _liminf, double _limsup, double numpontos, out List<double> saida)
        {
            saida = new List<double>();
            double temp = _liminf;
            double acrescimo = ((_limsup - _liminf) / (numpontos-1));


            for (int i = 0; i < (numpontos); i ++  )
            {
                saida.Add(temp);
                temp += acrescimo;
            } 

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

        void Escreve_dados_espessura(List<string> list1, string nome )
        {

            //...
            string separator = "\n";
            using (StreamWriter writer = new StreamWriter(nome))
            {
                //writer.Write(opcao_metodologia.ToString()); /// opcao metodologia 
                //writer.Write(separator);
                for (int i = 0; i < list1.Count; i++)
                {
                    writer.Write(list1.ElementAt(i));
                    writer.WriteLine();
                }
            }
        }

        
        void Calcula_Newton(List<double> en, List<double> cont, int ordematual, out List<double> _saida, out double _valor_a_armazenar, int ordem )
        {
            
            /// feitas e de quanto em quanto eu pulo
            /// no indice da subtracao do denominador
            _saida = new List<Double>();
            _valor_a_armazenar = 0.0;

            double conta = 0.0;

            for (int j = 0; j < ((ordem-1)-(ordematual)); j++)
            {
                  int indice=  j + 1;
                  int indice2 = (ordematual + 1) + j; 

                conta = (cont.ElementAt(indice) - cont.ElementAt(j)) / (en.ElementAt(indice2) - en.ElementAt(j));

                //MessageBox.Show(conta.ToString() + " " + (_ent2.ElementAt(i + 1) - _ent2.ElementAt(i)).ToString() + " " + (_ent1.ElementAt(ordem + i) - _ent1.ElementAt(i)).ToString());
                _saida.Add(conta);
            }
            if (_saida.Count() > 0) 
                _valor_a_armazenar = _saida.ElementAt(0);

        }

        private void Interp_New_Dif_Div_func(List<double> partenergia, List<double> partcontagem, out List<double> _saidaenergia, out List<double> _saidacontagem, List<double> energinterpolada ,int  inic , int numdadosparticao)
        {
            /// 
            /// adiciona saida energia e saidacontagem a dois novos vetores
            /// 
            /// escreve os dois novos vetores no arquivo 


            double menor, maior, armazenar;
            List<double> armazenados = new List<Double>();
            List<double> temporario = new List<Double>();
            _saidacontagem = new List<double>();
            _saidaenergia = new List<double>();
            _saidaenergia.Clear();
            _saidacontagem.Clear();
            /// pegando o menor e o maior valor no vetor x de entrada 
         //  menor = energia.Min();
         //   maior = energia.Max();

            /// tratar para nao receber null 
          

            /// gerando n pontos igualmente espaçados e armazena no vetor _saidaenergia 
            /// 

         /// Gera_Intervalo(menor, maior, numpontos, out _saidaenergia); // 5 intervalos

            int tamdados = partenergia.Count();  /// 5 primeiros dados interpolados 

            int ordem = partenergia.Count(); /// 5 primeiros dados interpolados 

            armazenados.Add(partcontagem.ElementAt(0)); /// pontos interpolados 

            double valor = armazenados.ElementAt(0); /// primeiro ponto da funcao interpolatoria já é conhecido
            double prod = 1.0;

            for (int i = 0; i < (ordem-1); i++) /// para cada ponto 
            {

                temporario.Clear();
                Calcula_Newton(partenergia, partcontagem, i , out temporario, out armazenar , ordem);
                partcontagem.Clear();
                /// melhorar ! funcao copia vetor 
                for (int k = 0; k < temporario.Count(); k++)
                {
                    partcontagem.Add(temporario.ElementAt(k));
                }
                armazenados.Add(armazenar); /// gera o vetor armazenados 
            }
            // a funcao interpolatoria já eh parcialmente conhecida neste momento do programa 

            /// aplicando o valor das energias interpoladas e obtendo a contagem interpolada 
            /// 
            for (int j = 0; j < (energinterpolada.Count); j++) /// para cada energia interpolada 
            {

                if (energinterpolada.ElementAt(j) >= partenergia.ElementAt(0) && energinterpolada.ElementAt(j) < partenergia.ElementAt(ordem - 1)) 
                {
                    for (int i = 1; i < (numdadosparticao); i++) /// calcular a contagem interpolada 
                    {
                        prod *= (energinterpolada.ElementAt(j) - partenergia.ElementAt(i));
                        valor += (armazenados.ElementAt(i) * prod);

                    }
                    //_saidaenergia.Add(energinterpolada.ElementAt(j));
                    _saidacontagem.Add(valor); /// adicionando ao vetor 
                    //MessageBox.Show(_saidacontagem.ElementAt(j).ToString()); 
                    valor = armazenados.ElementAt(0); ; /// reiniciando valor 
                    prod = 1.0;  /// reinciando 
                } 
            }

          
        }

        private void Interp_New_Dif_Div(List<double> energia, List<double> contagem, string nome_arq_saida, out List<double> _saidaenergia, out List<double> _saidacontagem)
        {
            _saidaenergia = new List<Double>();
            _saidacontagem = new List<double>();
            List<double> partenergia = new List<Double>();
            List<double> partcontagem = new List<Double>();
            List<double> saidapartenergia = new List<Double>();
            List<double> saidapartcontagem = new List<Double>();

            
            /// quebra o intervalo em particoes de 5 dados 
            /// 

            double menor, maior;
            int numpontosinterp = Convert.ToInt32(textBox15.Text);

            menor = energia.Min();
            maior = energia.Max();
            Gera_Intervalo(menor, maior, numpontosinterp, out _saidaenergia); /// divide o intervalo em 1000 pontos 


            int numdadosparticao = 5; //// numero de dados na particao
            int numparticoes = (energia.Count / (numdadosparticao-1)); /// calcula o numnero de particoes 
            int resto = (energia.Count % numdadosparticao);
          

            int inic = 0;
            for (int i = 0; i < (numparticoes-1); i++) /// depois melhorar , pensar como incluir a ultima particao 
            {
                    partenergia.Clear();
                    partcontagem.Clear();
                    
                    for (int j = 0; j < numdadosparticao; j++)
                    {
                        partenergia.Add(energia.ElementAt(inic + j));
                        partcontagem.Add(contagem.ElementAt(inic + j));
                    }
                    Interp_New_Dif_Div_func(partenergia,  partcontagem, out saidapartenergia, out saidapartcontagem, _saidaenergia , inic , numdadosparticao );
                    inic += (numdadosparticao-1);
                    
                    for (int j = 0 ; j < saidapartcontagem.Count; j++ )
                    {
                       // _saidaenergia.Add(saidapartenergia.ElementAt(j)); 
                        _saidacontagem.Add(saidapartcontagem.ElementAt(j)); 
                    }
            
            }

            Escreve_arquivo(_saidaenergia, _saidacontagem, nome_arq_saida);


            /// executa o procedimento nas n particoes 
            /// 
            




           
        }


        //vetor a , vetor b (meio) , vetor c , vetor d ,(solucao), tamanho da matriz  ;

        /// <summary>
        ///  Tridiag : Resolve sistemas Tridiagonais.  Melhorar !! 
        /// </summary>
        /// <param name="_a"></param>
        /// <param name="_b"></param>
        /// <param name="_c"></param>
        /// <param name="_d"></param>
        /// <param name="_x"></param>
        /// <param name="_tamanho"></param>

        List<double> Tridiag(List<double> _a, List<double> _b, List<double> _c, List<double> _d, int _tamanho)
        {


            List<double> a = new List<Double>(_a);
            List<double> b = new List<Double>(_b);
            List<double> c = new List<Double>(_c);
            List<double> d = new List<Double>(_d);
            List<double> saida = new List<Double>(_tamanho);

            int n = _tamanho;

            // MessageBox.Show(" N =  "+  n.ToString());



            for (int k = 1; k < n; k++)
            {
                a[k] = a[k] / b[k - 1];
                b[k] = b[k] - a[k] * _c[k - 1];
                //ok // cout << _a[k] << " " << _b[k] << endl ;
            }


            for (int k = 1; k < n; k++)
            {
                d[k] = d[k] - (a[k] * d[k - 1]);
                // cout << _d[k] << endl ;// ok
            }



            //cout << _x[n-1] << endl ;

            for (int k = 0; k < n - 1; k++)
            {
                saida.Add(0.0);
                ///cout << " k " << k <<" " <<  _x[k] << endl ;
            }


            //saida.Count();



            saida.Insert(n - 1, d.ElementAt(n - 1) / b.ElementAt(n - 1));

            //MessageBox.Show(saida.ElementAt(n - 1).ToString()); 
            for (int k = n - 2; k >= 0; k--)
            {

                saida.Insert(k, (d.ElementAt(k) - (_c.ElementAt(k) * saida.ElementAt(k + 1))) / b.ElementAt(k));
                ///cout << " k " << k <<" " <<  _x[k] << endl ;
            }


            return saida;
        }

        private List<double> AdiantaIndice(List<double> S, int n)
        {
            List<double> Temp;
            Temp = new List<double>(n);


            Temp.Add(0.0); /// só para começar Temp um indice adiantado 
            for (int i = 0; i < (n - 1); i++) /// vai até (n-2)  no S e (n-1) no Temp 
            {

                Temp.Add(S.ElementAt(i)); /// temp [1] = S[0 ] e etc.... 

            }
            S.Clear();

            for (int i = 0; i < n; i++)
            {
                S.Add(Temp.ElementAt(i));
            }

            return S;
        }

        private double Elevado(double _n, int _p)
        {
            if (_p == 0) return ((double)1.0); // este double é necessario para dar a precisao
            if (_p > 0) return ((_n * Elevado(_n, _p - 1)));
            return Elevado(1.0 / _n, -_p - 1) / _n;
        }


     

       private Complex DFT( List<double> funcao, int k)
       {
           Complex sumFu ;
           int tam_interpp = funcao.Count();
           double tamdbl = Convert.ToDouble(tam_interpp);
           sumFu = new Complex(0.0, 0.0);
           for (int j= 0; j < tam_interpp; j++)
           {

               sumFu += (funcao.ElementAt(j) * Complex.Exp((-1 * Complex.ImaginaryOne * 2.0 * Math.PI * j * k) / tamdbl)); /// tentar mult de complexos

           }

           return (sumFu);
       }

       private Complex IDFT(List<Complex> funcao, int k)
       {
           Complex sumFu;
           int tam_interpp = funcao.Count();
           double tamdbl = Convert.ToDouble(tam_interpp);
           sumFu = new Complex(0.0, 0.0);
           for (int j = 0; j < tam_interpp; j++)
           {

               sumFu += (funcao.ElementAt(j) * Complex.Exp(( Complex.ImaginaryOne * 2.0 * Math.PI * j * k) / tamdbl)); /// tentar mult de complexos

           }
           return (Complex.Multiply(sumFu,1.0 /(tam_interpp)));
         
       }


       private void Interp_Fourier5(List<double> entdados, List<double> entfuncao, string nome_arq_saida, out List<double> _saidaenergia, out List<double> _saidacontagem) /// interpola a regiao do k alpha 
       {
           int numpontos;
           double menor, maior;


           _saidacontagem = new List<double>();
           _saidaenergia = new List<double>();

            _saidacontagem.Clear();
           _saidaenergia.Clear();


      
           //if (numpontos %2 != 0 )  numpontos--; //// transformando em par 

           int contagemdados = entdados.Count();
           
           List<double> dados = new List<double>(entdados);
           List<double> funcao = new List<double>(entfuncao);

           if (contagemdados % 2 != 0)  /// corrigindo a entrada para numero par de n 
           {
               dados.RemoveAt(entdados.Count() - 1);
               funcao.RemoveAt(entfuncao.Count() - 1);
           }
          


           /// pegando o menor e o maior valor no vetor x de entrada 
           menor = dados.Min();
           maior = dados.Max();


           /// tratar para nao receber null , numero de pontos para interpolacao escolhido pelo usuario 
           numpontos = Convert.ToInt32(textBox15.Text);



           /// gerando n pontos igualmente espaçados e armazena no vetor _saidaenergia 
           /// 

           Gera_Intervalo(menor, maior, numpontos, out _saidaenergia);
           
        
          // int tam_interp = _saidaenergia.Count();

           Complex temp, temp2, sum;

           temp2 = new Complex(0.0, 0.0);
           sum = new Complex(0.0, 0.0);

      
           List<double> tp = new List<double>();
           List<Complex> xp = new List<Complex>();
           List<Complex> yp = new List<Complex>();

        
          tp.Clear();
          yp.Clear();

         
           for (int i = 0; i < numpontos; i++)
           {
               tp.Add(entdados.ElementAt(0) + i * (_saidaenergia.ElementAt(i) - entdados.ElementAt(0)) / (maior - menor));
               
               xp.Add(0.0);
           }


           for (int i = 0; i < numpontos; i++)
           {
               yp.Add(0.0);
           }




           List<Complex> IDFTvec;
           List<Complex> DFTvec ;
           DFTvec = new List<Complex>();
           IDFTvec = new List<Complex>();


           int tamdados = funcao.Count();

           /// transformada e Coeficientes FU 
           /// 
           DFTvec.Clear();
           IDFTvec.Clear();
           for (int u = 0; u < tamdados; u++)
           {
               temp = new Complex(0.0, 0.0);
               temp = DFT(funcao, u);
               DFTvec.Add(temp);

               temp = IDFT(DFTvec, u);
               IDFTvec.Add(temp);
             
           }

               

           for (int u = 0; u < (tamdados/2)+1; u++)
           {
               yp[u]=DFTvec.ElementAt(u);
           }
           int inic = numpontos - (tamdados / 2) + 1;
           int inicDTF = tamdados / 2 + 1; 
           int cont = 0;
           for (int u = inic; u < numpontos; u++)
           {
               yp[u] = DFTvec.ElementAt(inicDTF+cont);
               cont++;
           }


           for (int u = 0; u < numpontos; u++)
           {
               xp[u] = IDFT(yp, u);
               _saidacontagem.Add((xp.ElementAt(u).Real) * (Convert.ToDouble(numpontos) / Convert.ToDouble(tamdados)));
               //_saidaenergia.Add(tp.ElementAt(u));
               
           }



           tp.Count();
           Escreve_arquivo(_saidaenergia, _saidacontagem, nome_arq_saida);

       }


       private void Interp_Fourier4(List<double> entdados, List<double> entfuncao, string nome_arq_saida, out List<double> _saidaenergia, out List<double> _saidacontagem) /// interpola a regiao do k alpha 
       {
           int numpontos;
           double menor, maior;







           _saidacontagem = new List<double>();
           _saidaenergia = new List<double>();

           _saidacontagem.Clear();
           _saidaenergia.Clear();


           //if (numpontos %2 != 0 )  numpontos--; //// transformando em par 

           int contagemdados = entdados.Count();
           List<double> dados = new List<double>(entdados);
           List<double> funcao = new List<double>(entfuncao);

           if (contagemdados % 2 != 0)  /// corrigindo a entrada para numero par de n 
           {
               dados.RemoveAt(entdados.Count() - 1);
               funcao.RemoveAt(entfuncao.Count() - 1);
           }


           /*
           funcao.Clear();
           dados.Clear();

           
           funcao.Add(0.3);
           funcao.Add(0.6);
           funcao.Add(0.8);
           funcao.Add(0.5);
           funcao.Add(0.6);
           funcao.Add(0.4);
           funcao.Add(0.2);
           funcao.Add(0.3);


           dados.Add(0.0);
           dados.Add(0.125);
           dados.Add(0.25);
           dados.Add(0.375);
           dados.Add(0.5);
           dados.Add(0.625);
           dados.Add(0.75);
           dados.Add(0.875);
           */


           /// pegando o menor e o maior valor no vetor x de entrada 
           menor = dados.Min();
           maior = dados.Max();


           /// tratar para nao receber null , numero de pontos para interpolacao escolhido pelo usuario 
           numpontos = Convert.ToInt32(textBox15.Text);



           /// gerando n pontos igualmente espaçados e armazena no vetor _saidaenergia 
           /// 

           Gera_Intervalo(menor, maior, numpontos, out _saidaenergia);
           int tam_interp = _saidaenergia.Count();
           Double tam_interpdbl = Convert.ToDouble(tam_interp);



           Complex temp, temp2, sum;

           temp2 = new Complex(0.0, 0.0);
           sum = new Complex(0.0, 0.0);


           List<Complex> vec_comp, vec_comp2;
           vec_comp = new List<Complex>();
           vec_comp2 = new List<Complex>();


           int tamdados = funcao.Count();

           /// transformada e Coeficientes FU 
           /// 
           vec_comp.Clear();
           for (int u = 0; u < tamdados; u++)
           {
               temp = new Complex(0.0, 0.0);
               temp = DFT(funcao, u);
               vec_comp.Add(temp);
           }
           /// verificar com o pdf salvador pag 12 
           double sum2, temp3;

           
           double raizn = Math.Sqrt(Convert.ToDouble(tamdados));
           double pri, ult, ampter, ampcos, ampsin, pos;
           /// Transformada inversa 
           /// 

           pri =  ( vec_comp.ElementAt(0).Real / raizn );

           int meio = (tamdados / 2);

           ampter = (vec_comp.ElementAt(meio).Real  / raizn);
          



               for (int i = 0; i < tamdados; i++)
               {

                   //  pos = (_saidaenergia.ElementAt(i) - _saidaenergia.ElementAt(0)) / (maior - menor);

                   pos = i / Convert.ToDouble(funcao.Count());

                   sum2 = 0.0;



                   for (int k = 1; k <= (meio - 1); k++) /// só para pares ,ver depois para impares 
                   {
                       ampcos = (2.0 / raizn) * vec_comp.ElementAt(k).Real;
                       ampsin = (2.0 / raizn) * vec_comp.ElementAt(k).Imaginary;

                       //pos = (_saidaenergia.ElementAt(i)) / maior; /// normalizando 



                       temp3 = (ampcos * Math.Cos(2.0 * Math.PI * k * pos)) - (ampsin * Math.Sin(2.0 * Math.PI * k * pos));
                       sum2 += temp3;
                   }

                   //pos = (_saidaenergia.ElementAt(i)) / maior; /// normalizando 
                   // pos = (_saidaenergia.ElementAt(i) - _saidaenergia.ElementAt(0)) / (maior - menor); /// normalizando 
                   /// 

                   ult = ampter * Math.Cos(tamdados * Math.PI * (pos));

                   temp3 = (pri + sum2 + ult);

                   _saidacontagem.Add(temp3);
               }


               

           Escreve_arquivo(_saidaenergia, _saidacontagem, nome_arq_saida);

       }


      

       private void Interp_Fourier3(List<double> entdados, List<double> entfuncao, string nome_arq_saida, out List<double> _saidaenergia, out List<double> _saidacontagem) /// interpola a regiao do k alpha 
       {
           int numpontos;
           double menor, maior;


          
          
           


           _saidacontagem = new List<double>();
           _saidaenergia = new List<double>();

           _saidacontagem.Clear();
           _saidaenergia.Clear();


           //if (numpontos %2 != 0 )  numpontos--; //// transformando em par 

           int contagemdados = entdados.Count();
           List<double> dados = new List<double>(entdados);
           List<double> funcao = new List<double>(entfuncao);

           if (contagemdados %2 != 0)  /// corrigindo a entrada para numero par de n 
           {
               dados.RemoveAt(entdados.Count() - 1);
               funcao.RemoveAt(entfuncao.Count() - 1);
           }

           

           funcao.Clear();
           dados.Clear();


           funcao.Add(0.3);
           funcao.Add(0.6);
           funcao.Add(0.8);
           funcao.Add(0.5);
           funcao.Add(0.6);
           funcao.Add(0.4);
           funcao.Add(0.2);
           funcao.Add(0.3);


           dados.Add(0.0);
           dados.Add(1.0/8.0);
           dados.Add(2.0 / 8.0);
           dados.Add(3.0 / 8.0);
           dados.Add(4.0 / 8.0);
           dados.Add(5.0 / 8.0);
           dados.Add(6.0 / 8.0);
           dados.Add(7.0 / 8.0);

          



           /// pegando o menor e o maior valor no vetor x de entrada 
           menor = dados.Min();
           maior = dados.Max();
           
        
           /// tratar para nao receber null , numero de pontos para interpolacao escolhido pelo usuario 
           numpontos = Convert.ToInt32(textBox15.Text);

           

           /// gerando n pontos igualmente espaçados e armazena no vetor _saidaenergia 
           /// 

           Gera_Intervalo(menor, maior, numpontos, out _saidaenergia);
           int tam_interp = _saidaenergia.Count();
           Double tam_interpdbl = Convert.ToDouble(tam_interp);

         

           Complex temp, temp2, sum;

           temp2 = new Complex(0.0, 0.0);
           sum = new Complex(0.0, 0.0);


           List<Complex> vec_comp, vec_comp2;
           vec_comp = new List<Complex>();
           vec_comp2 = new List<Complex>();

                                             
           int tamdados = funcao.Count(); 

           /// transformada e Coeficientes FU 
           /// 
           vec_comp.Clear();
           for (int u = 0; u < tamdados; u++)
           {
               temp = new Complex(0.0, 0.0);
               temp = DFT(funcao, u);
               vec_comp.Add(temp);
           }
           /// verificar com o pdf salvador pag 12 
           double sum2, temp3; 

           double raiz = Math.Sqrt(dados.Count());
           double pri, ult , ampter , ampcos, ampsin , pos ;  
           /// Transformada inversa 
           /// 

           pri = (vec_comp.ElementAt(0).Real / raiz);

           int meio = (tamdados / 2); 

           ampter = vec_comp.ElementAt(meio).Real / raiz;

           for (int i = 0; i < tam_interp; i++)
           {

               //pos = ((_saidaenergia.ElementAt(i) - _saidaenergia.ElementAt(0)) / (maior - menor)); /// normalizando a entrada 
                                                                                                    /// 
           
              // pos = (_saidaenergia.ElementAt(i)) / maior; /// normalizando                                                                               
              // pos = (_saidaenergia.ElementAt(i)) ; /// normalizando                                                                               
               sum2 = 0.0;
               pos = dados.ElementAt(0);
               
               for (int k = 1; k <= ((tamdados/2)-1) ; k++) /// só para pares ,ver depois para impares 
               {
                   pos = _saidaenergia.ElementAt(0) + k * ((_saidaenergia.ElementAt(i) - _saidaenergia.ElementAt(0)) / raiz); /// normalizando a entrada 
                   ampcos = (2.0 / raiz) * vec_comp.ElementAt(k).Real;
                   ampsin = 1.0*(2.0 / raiz) * vec_comp.ElementAt(k).Imaginary;
  
                   temp3 = (ampcos* Math.Cos(2.0 * Math.PI * k *pos)) - (ampsin* Math.Sin(2.0 * Math.PI * k * pos));
                   sum2 += temp3;
               }
              // pos = _saidaenergia.ElementAt(0) + (tamdados/2)-1* ((_saidaenergia.ElementAt(i) - _saidaenergia.ElementAt(0)) / raiz); /// normalizando a entrada 
               //pos = (_saidaenergia.ElementAt(i)) / maior; /// normalizando 
              // pos = (_saidaenergia.ElementAt(i) - _saidaenergia.ElementAt(0)) / (maior - menor); /// normalizando 
                                                                                        /// 
                
               ult = ampter * Math.Cos(tamdados * Math.PI * (pos));

               temp3 = (pri + sum2 + ult) ;

               _saidacontagem.Add(temp3);
           }


           Escreve_arquivo(_saidaenergia, _saidacontagem, nome_arq_saida);

       }


      



        private void Interp_Splines_Natural(List<double> dados, List<double> funcao, string nome_arq_saida, out List<double> _saidaenergia, out List<double> _saidacontagem)
        {
            int numpontos;
            double menor, maior;
            List<double> temporario = new List<Double>();
            _saidacontagem = new List<double>();
            _saidaenergia = new List<double>();

            /// pegando o menor e o maior valor no vetor x de entrada 
            menor = dados.Min();
            maior = dados.Max();

            /// tratar para nao receber null , numero de pontos para interpolacao escolhido pelo usuario 
            numpontos = Convert.ToInt32(textBox15.Text);

            /// gerando n pontos igualmente espaçados e armazena no vetor _saidaenergia 
            /// 

            Gera_Intervalo(menor, maior, numpontos, out _saidaenergia);

            int tamdados = _saidaenergia.Count();  /// numero de pontos no vetor energia interpolado 

            int n = dados.Count();                   /// numero de pontos no vetor energia não interpolado 
            // MessageBox.Show(" N vale = " + n.ToString()); 
            int tam = n - 2;                            ///numero de variaeis que preciso para resolver o sistemaa
            // MessageBox.Show(" tam = " + tam.ToString());
            List<double> x, sol, y, h, lambda, a, b, c, d, lambda2, meio, S, saiday;



            x = new List<double>(dados);

            y = new List<double>(funcao);

            h = new List<double>(n);

            a = new List<double>(n);
            b = new List<double>(n);
            c = new List<double>(n);
            d = new List<double>(n);

            S = new List<double>(n);


            /// sistema 

            lambda = new List<double>(tam);
            lambda2 = new List<double>(tam);
            meio = new List<double>(tam);
            sol = new List<double>(tam);

            saiday = new List<double>(tamdados);

            // double delta=0.01; // distancia entre pontos no plot
            // double px; // ponto a ser interpolado no plot
            double pt, st, tt, qt; // termos do spline cubico
            int k = 0, i = 0, j = 0; // contador

            // MessageBox.Show ("Aqui" ); 
            //MessageBox.Show(x.Count.ToString());
            //MessageBox.Show(y.Count.ToString());
            //MessageBox.Show(n.ToString()); 

            /// ACHO que o erro está aqui !! 
            for (i = 0; i < (n - 1); i++) // i = 0 ... n-2
            {

                j = i + 1;
                h.Add(x[j] - x[i]); //  h[1] = x[1] - x[0] ... // ok
                // MessageBox.Show(h[i].ToString());
                //cout << i << " " << h[i] << endl;
            }

            for (i = 0; i < tam; i++) // i = 0 ... n-2
            {
                meio.Add(4.0000);
                lambda.Add(1.0000);
                lambda2.Add(1.0000);

                //cout << lambda [i] << " " << meio [i] << " " << lambda2[i] << endl;
            }

            for (i = 0; i < tam; i++) // i = 0 ... n-1
            {

                pt = 6.0 / (h.ElementAt(i) * h.ElementAt(i));
                sol.Add(pt * (y.ElementAt(i + 2) - (2 * y.ElementAt(i + 1)) + y.ElementAt(i)));
                //cout << "sol "<<sol[i] << endl ;

            }

            S = Tridiag(lambda, meio, lambda2, sol, tam);

            double[] M = new double[n];

            //List<double> M = new List<double>(n);

            M[0] = 0;
            S.CopyTo(1, M, 1, n - 2);
            M[n - 1] = 0;



           // for (i = 0; i < n; i++)
            {
                ///cout << "M[" << i + 1 << "]=" << setprecision(5) << S[i] << endl;
            }


            //calculando os coeficientes :
            double temp;
            for (i = 0; i < n - 2; i++)
            {

                a.Add((M[i + 1] - M[i]) / (6.0000 * h[i]));
                b.Add((M[i]) / (2.000));

                temp = ((M[i + 1] + (2.0000 * M[i])) / 6.0000) * h[i];

                c.Add(((y[i + 1] - y[i]) / (h[i])) - temp);
                d.Add(y[i]);
                //cout << a[i] << " " << b[i] << " " << c[i] << " " << d[i] << " " << temp << endl;
                //cout << px -x[0] << endl ;
            }

            _saidacontagem.Clear();
            i = 0;
            for (k = 0; k < (n - 2); k++)
            {

                do
                {
                    pt = (a.ElementAt(k) * Elevado((_saidaenergia.ElementAt(i) - x.ElementAt(k)), 3));
                    st = (b.ElementAt(k) * Elevado((_saidaenergia.ElementAt(i) - x.ElementAt(k)), 2));
                    tt = (c.ElementAt(k) * (_saidaenergia.ElementAt(i) - x.ElementAt(k)));
                    qt = (d.ElementAt(k));

                    //  if(k==0 && px==5)
                    //    cout << "termos !!! " << qt ;
                    _saidacontagem.Add(pt + st + tt + qt);


                    i++;
                }
                while (_saidaenergia.ElementAt(i) <= x.ElementAt(k + 1));

            }

            k = n - 3;
            do
            {

                pt = (a.ElementAt(k) * Elevado((_saidaenergia.ElementAt(i) - x.ElementAt(k)), 3));
                st = (b.ElementAt(k) * Elevado((_saidaenergia.ElementAt(i) - x.ElementAt(k)), 2));
                tt = (c.ElementAt(k) * (_saidaenergia.ElementAt(i) - x.ElementAt(k)));
                qt = (d.ElementAt(k));

                //  if(k==0 && px==5)
                //    cout << "termos !!! " << qt ;
                _saidacontagem.Add(pt + st + tt + qt);
                i++;
            }
            while (i < _saidaenergia.Count);


            Escreve_arquivo(_saidaenergia, _saidacontagem, nome_arq_saida);


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
        /// e se ao inves de gerar um intervalo igualmente espacado eu forçasse a interpolacao a passar pelo ponto da largura a meia altura ?  

        private void Interp_Lagrange(List<double> _x, List<double> _y, string nome_arq_saida, out List<double> _saidaenergia, out List<double> _saidacontagem)
        {
            int numpontos;
            double menor, maior;
            _saidacontagem = new List<double>();

            /// pegando o menor e o maior valor no vetor x de entrada 
            menor = _x.Min();
            maior = _x.Max();

            /// tratar para nao receber null 
            numpontos = Convert.ToInt32(textBox15.Text);

            /// gerando n pontos igualmente espaçados e armazena no vetor _saidaenergia 
            /// 

            Gera_Intervalo(menor, maior, numpontos, out _saidaenergia);

            int tamdados = _saidaenergia.Count();

            int tamx = _x.Count();


            //MessageBox.Show("Inicio Lagrange");

            // calculo dos polinomios L
            double somatorio = 0.0;
            double prod = 1.0;

            for (int k = 0; k < tamdados; k++)
            {
                somatorio = 0.0;
                for (int i = 0; i < tamx; i++)
                {
                    prod = 1.0;
                    //calculo do Lj
                    for (int j = 0; j < tamx; j++)
                    {
                        if (j != i)
                        {
                            prod *= (_saidaenergia.ElementAt(k) - _x.ElementAt(j)) / (_x.ElementAt(i) - _x.ElementAt(j));

                        }
                    }
                    //System.Console.WriteLine(i + " " + prod); 
                    somatorio += _y.ElementAt(i) * prod;

                }
                _saidacontagem.Add(somatorio);
            }

            //MessageBox.Show("Fim LAgrange");

            /// escrever em um arquivo a interpolação feita 
            /// 
            Escreve_arquivo(_saidaenergia, _saidacontagem, nome_arq_saida);

        }



        /// <summary>
        /// Le um arquivo do DPMCA
        ///  Preenche os vetores 
        ///  List<int> canal_calibracao  -> canais utilizados na calibracao 
        ///  List<double> energia_calibracao  -> energia utilizada na calibracao 
        ///  List<int> contagens_descalibradas  -> contagens sem calibracao 
        /// </summary>
        /// <param name="myStream"></param>
        /// <param name="nome"></param>
        void LeArquivo(Stream myStream, string nome)
        {
            string line;                        /// string para guardar as linhas enquanto sao lidas 
            /// Realiza a calibracao Quadratica 
            /// retorna os valores A,B,C calibrados 
            //  Calibracao_Quadratica(dados_descalibrados,calibracao, A, B, C);

            if (myStream != null)
            {
                using (myStream)
                {
                    /// vou tentar fazer tudo em c#... 
                    /// MessageBox.Show(openFileDialog1.FileName); 
                    //string text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                    nome_arquivo = nome;
                    System.IO.StreamReader file = new System.IO.StreamReader(nome_arquivo);
                    //textBoxCalib.Clear();
                    while ((line = file.ReadLine()) != null)
                    {


                        if (line.StartsWith("GAIN"))
                        {
                           // textBoxganho.Text = line.Substring(7, (line.Length - 7));/// a partir do caractere 5 pega até o fim  caracteres /// pegando o numero de canais
                        }


                        if (line.StartsWith("START_TIME"))
                        {
                           // textBoxhora.Text = line.Substring(13, (line.Length - 13));/// a partir do caractere 5 pega até o fim  caracteres /// pegando o numero de canais
                        }


                        if (line.StartsWith("LABEL - Channel") || line.StartsWith("LABEL - keV") || line.StartsWith("LABEL - KeV"))
                        {
                            canal_calibracao.Clear();
                            energia_calibracao.Clear();

                            line = file.ReadLine();
                            while (!line.StartsWith("<<")) // eqnuanto nao encontrar uma proxima sessao 
                            {
                                string[] valores = line.Split(' '); /// pega os dois valores da calibracao 
                                canal_calibracao.Add(StrtoInt(valores[0]));
                                energia_calibracao.Add(StrtoDbl(valores[1]));
                                //MessageBox.Show( valores[0] + " " + valores[1] );
                                line = file.ReadLine();

                            }


                        }

                        if (line.StartsWith("<<DATA>>"))
                        {
                            line = file.ReadLine();
                            contagens_descalibradas.Clear();
                            while (!line.StartsWith("<<END>>")) // eqnuanto nao encontrar uma proxima sessao 
                            {
                                contagens_descalibradas.Add(StrtoInt(line));
                                //MessageBox.Show(line);
                                line = file.ReadLine();
                            }
                        }


                    }
                }
            }
        }


        ///List<int> canal_calibracao  -> canais utilizados na calibracao 
        ///  List<double> energia_calibracao  -> energia utilizada na calibracao 
        ///  List<int> contagens_descalibradas  -> contagens sem calibracao 
        ///  http://www.azdhs.gov/lab/documents/license/resources/calibration-training/12-quadratic-least-squares-regression-calib.pdf

        void Calibra(out double A, out double B, out  double C) /// Busca a calibracao 
        {

            /// x = List<int> canal_calibracao  
            /// y =  List<double> energia_calibracao  -> energia utilizada na calibracao 
            C = 0.0;
            B = 0.0;
            A = 0.0;
            int n = canal_calibracao.Count();

            if (n<=0 ) MessageBox.Show("Erro nos parametro n  da leitura do arquivo DPMCAA" );

            /// Linear 
            if (n==2)
            {
                /// so para me organizar 
                List<int> x = canal_calibracao.ToList();
                List<double> y = energia_calibracao.ToList();
                double xi = 0.0, yi = 0.0;

                for (int i = 0; i < n; i++)
                {
                    xi = x.ElementAt(i);
                    yi = y.ElementAt(i);
                }

                B = (y.ElementAt(1) - y.ElementAt(0)) / (x.ElementAt(1) - x.ElementAt(0));
                A = y.ElementAt(0) - B * x.ElementAt(0);

            }
            /// quadratica 
            if (n > 2)
            {


                double Sxi = 0.0, Sxi_2 = 0.0, Sxi_3 = 0.0, Sxi_4 = 0.0,
                       Syi = 0.0,
                       Sxi_2yi = 0.0, Sxiyi = 0.0;

                double Sxx = 0.0, Sxy = 0.0, Sxx_2 = 0.0, Sx2y = 0.0, Sx2x2 = 0.0;

                double xi = 0.0, yi = 0.0;

                /// so para me organizar 
                List<int> x = canal_calibracao.ToList();
                List<double> y = energia_calibracao.ToList();

                for (int i = 0; i < n; i++)
                {
                    xi = x.ElementAt(i);
                    yi = y.ElementAt(i);

                    Sxi += xi;
                    Sxi_2 += xi * xi;
                    Sxi_3 += xi * xi * xi;
                    Sxi_4 += xi * xi * xi * xi;

                    Syi += yi;

                    Sxiyi += xi * yi;
                    Sxi_2yi += xi * xi * yi;

                }


                Sxx = Sxi_2 - (Sxi * Sxi / n);
                Sxy = (Sxiyi) - (Sxi * Syi / n);
                Sxx_2 = Sxi_3 - (Sxi * Sxi_2 / n);
                Sx2y = Sxi_2yi - (Sxi_2 * Syi / n);
                Sx2x2 = Sxi_4 - (Sxi_2 * Sxi_2 / n);


                /// C x^2 + B x + A 
                /// 
              
                double temp = (Sxx * Sx2x2);
                double temp2 = (Sxx_2 * Sxx_2);

                if ((temp - temp2) == 0)
                {
                    MessageBox.Show("Erro nos parametros da leitura do arquivo DPMCAA ");

                }
                else
                {
                    C = ((Sx2y * Sxx) - (Sxy * Sxx_2)) / (temp - temp2);
                    B = ((Sxy * Sx2x2) - (Sx2y * Sxx_2)) / (temp - temp2);
                    A = (Syi / n) - (B * (Sxi / n)) - (C * (Sxi_2 / n));
                }

          
            }



            
            ///MessageBox.Show(A.ToString() + " " + B.ToString()  + " " + C.ToString()   );

        }

        /// Passa as contagens não interpoladas 
        /// pela interpolacao e armazena nos 
        ///  vetores energia e contagem  
        ///  
        void Preenche_EnergiaXContagem(double A, double B, double C)
        {

            int tam = contagens_descalibradas.Count;
            double cont, en;
            //Arquivo_aberto.Clear();
            /// MessageBox.Show(A.ToString() + " " + B.ToString() + " " + C.ToString());

            string path = Directory.GetCurrentDirectory();

            System.IO.StreamWriter file = new System.IO.StreamWriter(path + "\\dpmca.out"); /// sobrescreve
            string linha;

            for (int i = 0; i < tam; i++)
            {
                cont = contagens_descalibradas.ElementAt(i);
                contagem.Add(cont);
                contagem_original.Add(cont);
                en = A + B * i + C * i * i;  /// utilizando o resultado da calibracao 
                ///
                energia.Add(en);
                energia_original.Add(en);

               // Arquivo_aberto.AppendText(energia.ElementAt(i).ToString() + " " + contagem.ElementAt(i).ToString());
               // Arquivo_aberto.AppendText(System.Environment.NewLine);

                linha = energia.ElementAt(i).ToString() + " " + contagem.ElementAt(i).ToString();
                file.WriteLine(linha);
            }

            file.Close();


        }


        /// <summary>
        ///  opcao DPMCA 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void dPCMAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            opcao_programa = 1; /// DPCMA selecionado 

            double A = 0.0, B = 0.0, C = 0.0;  // constantes da calibracao 

            List<Int64> canal = new List<Int64>(); /// vetor que guardara as contagens em cada canal
            LimpaTudo();
            LimpaOriginal(); 
            OpenFileDialog openFileDialog1 = new OpenFileDialog();


            openFileDialog1.InitialDirectory = dir_inicial;
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;



            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    /// Tenta ler o arquivo 
                    LeArquivo(openFileDialog1.OpenFile(), openFileDialog1.FileName);
                    Calibra(out A, out B, out C); /// Busca a calibracao 
                    Preenche_EnergiaXContagem(A, B, C);  /// Passa as contagens não interpoladas 
                    /// pela interpolacao e armazena nos 
                    ///  vetores energia e contagem  

                    /*textBox26.Text = "12000";
                    textBox25.Text = "1000";
                    textBox22.Text = "6.55";
                    textBox24.Text = "7.2";
                    */
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                }
            }

            /*
            if (textBoxI0.Text != null)
            {

                //textBoxI0.Text = canal[1020].ToString(); /// contagens do canal do k aplha do ferro 6,4 keV canal = 1020
                //textBoxIs.Text = canal[1371].ToString(); /// contagens do canal do k aplha da cobertura por
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
                //double Is, Ic, theta, espE0, espKalpha;
                //Ic = Convert.ToDouble(textBoxI0.Text);
                //Is = Convert.ToDouble(textBoxIs.Text);
                // theta = Convert.ToDouble(textBoxtheta.Text);
                //espE0 = Convert.ToDouble(textBoxmuE0.Text) * 7.13;
                //espKalpha = Convert.ToDouble(textBoxmuKAlpha.Text) * 7.13;
                //temp = Math.Log((Is / Ic), 2) * Math.Cos(theta * Math.PI / 180) / (espE0 + espKalpha);
                //textBoxespessura.Text = temp.ToString();
            }


            */

        }






        /// <summary>
        /// OPÇÂO GEANT4 
        ///  le o arquivo retornado em openDialog e guarda energia e contagem nos vetores energia e contagem nao interpolados 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void geant4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            opcao_programa = 2; /// Geant4 selecionado 

            Stream myStream = null;
            List<double> canal = new List<double>(); /// vetor que guardara as contagens em cada canal

            /// Abrindo o arquivo 
            
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = dir_inicial;
          
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
           // openFileDialog1.RestoreDirectory = true;


            if (openFileDialog1.ShowDialog() == DialogResult.OK)/// se abriu com sucesso 
            {
               
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        LimpaTudo();
                        LimpaOriginal();
                        using (myStream)
                        {

                            /// MessageBox.Show(openFileDialog1.FileName); 

                            nome_arquivo = openFileDialog1.FileName;

                            string text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                           // Arquivo_aberto.AppendText(nome_arquivo);
                           // Arquivo_aberto.AppendText(System.Environment.NewLine);
                           // Arquivo_aberto.AppendText(text);

                            string[] lines = System.IO.File.ReadAllLines(openFileDialog1.FileName);
                            int i = 0;

                            energia.Clear();
                            contagem.Clear();

                            ///abrindo o arquivo em duas colunas e convertendo para double 
                            foreach (string linha in lines)
                            {
                                string[] separado = linha.Split(new Char[] { ' ', '\t' }); /// separa a linha  tabulacao 

                                foreach (string dado in separado)   /// para cada dado na linha 
                                {

                                    if (dado.Trim() != "")  // se nao esta nulo 
                                    {
                                        if (i == 0) /// se é o primeiro dado adiciona para o vetor energia 
                                        {
                                            energia.Add(StrtoDbl(dado));  /// adiciona para o vetor energia nao interpolado 
                                            energia_original.Add(StrtoDbl(dado));  /// adiciona para o vetor energia nao interpolado 
                                            i = 1;
                                        }
                                        else  /// senao adiciona para o vetor contagem nao interpolado 
                                        {
                                            contagem.Add(StrtoDbl(dado));
                                            contagem_original.Add(StrtoDbl(dado));
                                            i = 0;
                                        }
                                    }

                                }



                            }


                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                }
            }
        }





        /// <summary>
        /// Le um arquivo do DPMCA
        ///  Preenche os vetores 
        ///  List<int> canal_calibracao  -> canais utilizados na calibracao 
        ///  List<double> energia_calibracao  -> energia utilizada na calibracao 
        ///  List<int> contagens_descalibradas  -> contagens sem calibracao 
        /// </summary>
        /// <param name="myStream"></param>
        /// <param name="nome"></param>
        void LeArquivoMCNPX(Stream myStream, string nome)
        {
            string line;                        /// string para guardar as linhas enquanto sao lidas 
            /// Realiza a calibracao Quadratica 
            /// retorna os valores A,B,C calibrados 
            //  Calibracao_Quadratica(dados_descalibrados,calibracao, A, B, C);

            if (myStream != null)
            {
                
                using (myStream)
               
                {
                    energia.Clear();
                    contagem.Clear();
                    /// vou tentar fazer tudo em c#... 
                    /// MessageBox.Show(openFileDialog1.FileName); 
                    //string text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                    nome_arquivo = nome;
                    System.IO.StreamReader file = new System.IO.StreamReader(nome_arquivo);
                    //textBoxCalib.Clear();
                    while ((line = file.ReadLine()) != null)
                    {


                        if (line.StartsWith(" cell  1   ") || line.StartsWith(" cell  2   "))
                        {
                            line = file.ReadLine(); /// palavra energia 
                            line = file.ReadLine(); /// palavra energia 
                            while (!line.StartsWith("      total")) // eqnuanto nao encontrar uma proxima sessao 
                            {
                                double num; 
                                //MessageBox.Show(line);
                                string[] valores = Regex.Split(line,"    ");
                               
                                //MessageBox.Show(valores[1]);
                                string[] valores2 = valores[1].Split(' '); /// pega os dois valores da calibracao  
                                //MessageBox.Show(valores2[0] );
                                num = Double.Parse(valores2[0], System.Globalization.NumberStyles.Float);
                                energia.Add(num*1000.0);
                                energia_original.Add(num * 1000.0);
                                //MessageBox.Show(num.ToString());
                                num = Double.Parse(valores2[3], System.Globalization.NumberStyles.Float);
                                contagem.Add(num * 1000.0);
                                contagem_original.Add(num * 1000.0);
                                //MessageBox.Show(num.ToString());
                              
                                line = file.ReadLine();

                            }
                        }

                    
                    }
                    
                }
               
            }
        }

        private void mCGolo64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            opcao_programa = 3; /// MCgolo  selecionado 
                                /// 
            Stream myStream = null;
            List<double> canal = new List<double>(); /// vetor que guardara as contagens em cada canal

            /// Abrindo o arquivo 

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = dir_inicial;
            
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
           // openFileDialog1.RestoreDirectory = true;
            
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LimpaTudo();
                    LimpaOriginal();
                    /// Tenta ler o arquivo 
                    LeArquivoMCNPX(openFileDialog1.OpenFile(), openFileDialog1.FileName);
                    
                    
                    /// pela interpolacao e armazena nos 
                    ///  vetores energia e contagem  

                    /*textBox26.Text = "12000";
                    textBox25.Text = "1000";
                    textBox22.Text = "6.55";
                    textBox24.Text = "7.2";
                    */
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                }
            }

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
      


        /// <summary>
        ///  Seleciona o Algoritmo Interpolador 
        /// </summary>
        /// <param name="algoritmo"></param>
        void Interpola(int algoritmo) /// com interpolacao 
        {


            kalpha_interp_energia.Clear();
            kalpha_interp_cont.Clear();

            kbeta_interp_energia.Clear();
            kbeta_interp_cont.Clear();

            if ((kalpha_energia.Count() <= 5) || (kbeta_energia.Count() <= 5))
            {
                MessageBox.Show("Erro Impossível Interpolar : Verifique a região do kAlpha ou Kbeta  ");

            }
            else
            {
                switch (comboBox2.SelectedIndex)
                {

                    case 1: /// Lagrange 
                        /// 
                        ///  interpola e guarda a interpolacao de um intervalo definido no form nos vetores de energia e contagem interpolados 
                        Interp_Lagrange(kalpha_energia, kalpha_contagem, "interpolacaokalpha.dat", out kalpha_interp_energia, out  kalpha_interp_cont); /// interpola a regiao do k alpha 
                        Interp_Lagrange(kbeta_energia, kbeta_contagem, "interpolacaokbeta.dat", out kbeta_interp_energia, out kbeta_interp_cont);   //// interpola a regiao do k beta 
                        break;
                    case 2: /// Newton
                            /// 
                        Interp_New_Dif_Div(kalpha_energia, kalpha_contagem, "interpolacaokalpha.dat", out kalpha_interp_energia, out kalpha_interp_cont); /// interpola a regiao do k alpha 
                        Interp_New_Dif_Div(kbeta_energia, kbeta_contagem, "interpolacaokbeta.dat", out kbeta_interp_energia, out  kbeta_interp_cont);   //// interpola a regiao do k beta 
                        break;
                    case 3:  /// Splines Cúbicos 
                        Interp_Splines_Natural(kalpha_energia, kalpha_contagem, "interpolacaokalpha.dat", out kalpha_interp_energia, out kalpha_interp_cont); /// interpola a regiao do k alpha 
                        Interp_Splines_Natural(kbeta_energia, kbeta_contagem, "interpolacaokbeta.dat", out kbeta_interp_energia, out  kbeta_interp_cont);   //// interpola a regiao do k beta
                        break;
                    //case 3:  /// Interpola todo o espectro com Splines 
                        //Interp_Splines_Natural(kalpha_energia, kalpha_contagem, "interpolaespecSpline.dat", out kalpha_interp_energia, out kalpha_interp_cont); /// interpola a regiao do k alpha 
                        /// Interp_Splines_Natural(kbeta_energia, kbeta_contagem, "interpolacaokbeta.dat", out kbeta_interp_energia, out  kbeta_interp_cont);   //// interpola a regiao do k beta
                      //  break;
                    case 4:  /// Interpola todo o espectro com Splines 
                        Interp_Fourier5(kalpha_energia, kalpha_contagem, "interpolacaokalpha.dat", out kalpha_interp_energia, out kalpha_interp_cont); /// interpola a regiao do k alpha 
                        Interp_Fourier5(kbeta_energia, kbeta_contagem, "interpolacaokbeta.dat", out kbeta_interp_energia, out  kbeta_interp_cont);   //// interpola a regiao do k beta
                        break;

                    case -1:  /// Erro 
                        MessageBox.Show("Escolha um Algoritmo de interpolação ");
                        break;
                }


            }





        }



       

        /// <summary>
        ///  
        /// cria dois subvetores de energia e contagem para interpolacao do pico do kaplha e k beta 
        ///  os subvetores kalpha e kbeta interpolados já estao armazenados quando fazemos a interpolacao 
        ///  
        /// </summary>
        private int  Calcula_regiao_Ka_Kb()
        {
            kalpha_energia.Clear();
            kalpha_contagem.Clear();

            kbeta_energia.Clear();
            kbeta_contagem.Clear();

            int posmin, posmax, quantidade;

            ///////////////
            // K alpha 
            /////////////
            /// Encontra a posicao da energia desejada no array energia e copia para um sub array kalpha_energia e k_alpha contagem 
            posmin = Encontra_Menor_que(energia, StrtoDbl(textBox6.Text));
            posmax = Encontra_Maior_que(energia, StrtoDbl(textBox22.Text));

            quantidade = posmax - posmin;

            if (quantidade < 0) /// teste de erro no kalpha , nao calcula 
            {
                MessageBox.Show("Erro ao encontrar para Kalpha  Entre posmin = " + posmin + " posmax= " + posmax + " no intervalo " + textBox6.Text + " e " + textBox22.Text + " Refaça o intervalo de Ka e Kb" );
                return 1;
            }
            else
            {
                /// pega uma subparte do vetor energia e guarda como os pontos necessários para calcular o kalpha interpolado 
                kalpha_energia = energia.GetRange(posmin, quantidade);
                kalpha_contagem = contagem.GetRange(posmin, quantidade);

                Escreve_arquivo(kalpha_energia, kalpha_contagem, "regiaokalpha.out");


                //MessageBox.Show("kalpha Posmin , cont_at_posmin , posmax ,cont_at_posmax,  quantidade " + energia.ElementAt(posmin).ToString() + "  " + contagem.ElementAt(posmin).ToString() + " " + energia.ElementAt(posmax).ToString() + " " + contagem.ElementAt(posmax).ToString() + " " + quantidade.ToString()); 

                ///////////////
                // K beta
                /////////////
                /// Encontra a posicao da energia desejada no array energia e copia para um sub array kbeta_energia e k_beta contagem 
                posmin = Encontra_Menor_que(energia, StrtoDbl(textBox23.Text));
                posmax = Encontra_Maior_que(energia, StrtoDbl(textBox24.Text));
                quantidade = posmax - posmin;
                
                if (quantidade < 0) /// teste de erro no kalpha , nao calcula 
                {
                    MessageBox.Show("Erro ao encontrar para Kbeta  Entre posmin = " + posmin + " posmax= " + posmax + " no intervalo " + textBox6.Text + " e " + textBox22.Text + " Refaça o intervalo de Ka e Kb");
                    return 1;
                }

                kbeta_energia = energia.GetRange(posmin, quantidade);
                kbeta_contagem = contagem.GetRange(posmin, quantidade);
                Escreve_arquivo(kbeta_energia, kbeta_contagem, "regiaokbeta.out");

                //MessageBox.Show("kalpha Posmin , cont_at_posmin , posmax ,cont_at_posmax,  quantidade " + energia.ElementAt(posmin).ToString() + "  " + contagem.ElementAt(posmin).ToString() + " " + energia.ElementAt(posmax).ToString() + " " + contagem.ElementAt(posmax).ToString() + " " + quantidade.ToString()); 

                //  MessageBox.Show("kalpha Posmin , cont_at_posmin , posmax ,cont_at_posmax,  quantidade " + energia.ElementAt(posmin).ToString() + "  " + contagem.ElementAt(posmin).ToString() + " " + energia.ElementAt(posmax).ToString() + " " + contagem.ElementAt(posmax).ToString() + " " + quantidade.ToString()); 



                maxen = energia.Max();
                maxcont = contagem.Max();
                return 0; 


            }



        }

        void LimpaOriginal()
        {
            contagem_original.Clear();
            energia_original.Clear();
        }
        void LimpaTudo()
        {

            
            
            

            kalpha_energia.Clear();
            kalpha_contagem.Clear();

            kbeta_energia.Clear();
            kbeta_contagem.Clear();

            kalpha_interp_energia.Clear();
            kalpha_interp_cont.Clear();

            kbeta_interp_energia.Clear();
            kbeta_interp_cont.Clear();

            pos_en_kalpha_min = 0;
            pos_en_kalpha_max = 0;
            pos_en_kbeta_min = 0;
            pos_en_kbeta_max = 0;
            largurakalpha = 0.0;
            largurakbeta = 0.0;

            maxen = 0;
            maxcont = 0;
            correcao_LMH_KA_KB = 0; 

            picos.Clear();
            canal_calibracao.Clear();
            energia_calibracao.Clear();
            contagens_descalibradas.Clear();
            //Arquivo_aberto.Clear();
            
          //  Interp_nodos_suave.Clear();
            nodo.Clear();
            Interp_nodos.Clear();

            chart1.Series["Plot"].Points.Clear();
            chart1.Series["Kalpha"].Points.Clear();
            chart1.Series["Kbeta"].Points.Clear();
            chart1.Series["Nodos"].Points.Clear();
            chart1.Series["InterpNodos"].Points.Clear();
            
            chart1.ResetAutoValues();
            
            
        }

        void GeraGrafico(int interp) /// interp = 0  sem interp intrp=1 com interp , -1 erro 
        {
            if (interp == -1 )  /// para evitar os erros de escala 
            {
               
               


                chart1.ChartAreas["ChartArea1"].AxisX.Minimum = 0 ;
                chart1.ChartAreas["ChartArea1"].AxisX.Maximum = 30;
                chart1.ChartAreas["ChartArea1"].AxisX.Interval = 0.1;

                chart1.ChartAreas["ChartArea1"].AxisY.Minimum = 0;
                chart1.ChartAreas["ChartArea1"].AxisY.Maximum = 30;
                chart1.ChartAreas["ChartArea1"].AxisX.Interval = 0.1;
                
                
                //chart1.Series["Plot"].Points.AddXY(1.0, 1.0);
                //chart1.Series["Plot"].Points.AddXY(1.2, 1.2);

                chart1.ChartAreas["ChartArea1"].AxisX.Minimum = Convert.ToDouble(textBox10.Text);
                chart1.ChartAreas["ChartArea1"].AxisX.Maximum = Convert.ToDouble(textBox16.Text);
                chart1.ChartAreas["ChartArea1"].AxisX.Interval = Convert.ToDouble(textBox17.Text);
                chart1.ChartAreas["ChartArea1"].CursorX.Interval = Convert.ToDouble(textBox17.Text);
                chart1.ChartAreas["ChartArea1"].AxisX.RoundAxisValues();
                chart1.ChartAreas["ChartArea1"].AxisY.Minimum = Convert.ToDouble(textBox27.Text);
                chart1.ChartAreas["ChartArea1"].AxisY.Maximum = Convert.ToDouble(textBox26.Text);

                for (int i = 0; i < energia.Count; i++)
                {

                    chart1.Series["Plot"].Points.AddXY(Math.Abs(energia.ElementAt(i)), Math.Abs(contagem_original.ElementAt(i)));
                }
            }


            if (energia.Count() > 0 && (interp != -1) ) /// para todos os casos , com e sem interpolacao 
            {
                //MessageBox.Show("Entrei ");
                if (textBox26.Text == "0" || textBox26.Text == "")
                    textBox26.Text = (maxcont + 0.1 * maxcont).ToString();
                chart1.Series["Plot"].Points.Clear();
                chart1.Series["Kalpha"].Points.Clear();
                chart1.Series["Kbeta"].Points.Clear();
                chart1.Series["Nodos"].Points.Clear();

                chart1.ChartAreas["ChartArea1"].AxisX.Minimum = Convert.ToDouble(textBox10.Text);
                chart1.ChartAreas["ChartArea1"].AxisX.Maximum = Convert.ToDouble(textBox16.Text);
                chart1.ChartAreas["ChartArea1"].AxisX.Interval = Convert.ToDouble(textBox17.Text);
                chart1.ChartAreas["ChartArea1"].CursorX.Interval = Convert.ToDouble(textBox17.Text);
                chart1.ChartAreas["ChartArea1"].AxisX.RoundAxisValues();


                chart1.ChartAreas["ChartArea1"].AxisY.Minimum = Convert.ToDouble(textBox27.Text);
                chart1.ChartAreas["ChartArea1"].AxisY.Maximum = Convert.ToDouble(textBox26.Text);
                 


                chart1.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "N" + textBox8.Text;


                chart1.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
                chart1.ChartAreas["ChartArea1"].CursorX.IsUserSelectionEnabled = true;
                chart1.ChartAreas["ChartArea1"].AxisY.ScaleView.Zoomable = true;
                chart1.ChartAreas["ChartArea1"].AxisX.ScaleView.Zoomable = true;



                chart1.ChartAreas["ChartArea1"].AxisX.LabelStyle.TruncatedLabels = true;

                if (checkBox4.Checked)
                    chart1.ChartAreas["ChartArea1"].AxisX.MajorGrid.Enabled = true;
                else
                    chart1.ChartAreas["ChartArea1"].AxisX.MajorGrid.Enabled = false;

                if (checkBox5.Checked)
                    chart1.ChartAreas["ChartArea1"].AxisY.MajorGrid.Enabled = true;
                else
                    chart1.ChartAreas["ChartArea1"].AxisY.MajorGrid.Enabled = false;


                if (checkBox1.Checked) /// Inserindo os nodos 
                {
                    if (nodo.Count() > 0 )
                    {
                        int pos;
                        double ene, conta; 
                        for (int i = 0; i < nodo.Count; i++)
                        {
                            pos = nodo.ElementAt(i);
                            ene = energia_original.ElementAt(pos);
                            conta = contagem_original.ElementAt(pos);
                            chart1.Series["Nodos"].Points.AddXY(Math.Abs(ene), Math.Abs(conta));
                            chart1.Series["Nodos"].Points[i].MarkerStyle = MarkerStyle.Cross;
                        }

                        for (int i = 0; i < Interp_nodos.Count; i++)
                        {
                             chart1.Series["InterpNodos"].Points.AddXY(Math.Abs(energia.ElementAt(i)), Math.Abs(Interp_nodos.ElementAt(i)));
                            
                        }

                    }
                    
                }

               



                if (checkBox6.Checked == false)  /// Inserir a funcao ? 
                {
                    if (checkBox8.Checked) /// plotar sem BG 
                    {
                        for (int i = 0; i < energia.Count; i++)
                        {

                            chart1.Series["Plot"].Points.AddXY(Math.Abs(energia.ElementAt(i)), Math.Abs(contagem_suave_sembg.ElementAt(i)));
                        }

                    }
                    else /// plotar a funcao com o background
                    {
                        for (int i = 0; i < energia.Count; i++)
                        {
                            chart1.Series["Plot"].Points.AddXY(Math.Abs(energia.ElementAt(i)), Math.Abs(contagem.ElementAt(i)));
                        }

                    }
                    
                }
                else
                {
                    chart1.ChartAreas["ChartArea1"].AxisY.ScrollBar.Enabled = true;

                }


                

                if (interp == 1)  /// se há interpolacao 
                {

                    for (int i = 0; i < Math.Min(kalpha_interp_energia.Count, kalpha_interp_cont.Count); i++)
                    {


                        chart1.Series["Kalpha"].Points.AddXY(Math.Abs(kalpha_interp_energia.ElementAt(i)), Math.Abs(kalpha_interp_cont.ElementAt(i)));
                    }

                    for (int i = 0; i < Math.Min(kbeta_interp_energia.Count, kbeta_interp_cont.Count); i++)
                    {
                        chart1.Series["Kbeta"].Points.AddXY(Math.Abs(kbeta_interp_energia.ElementAt(i)), Math.Abs(kbeta_interp_cont.ElementAt(i)));
                    }

                   

                }



                

            }
            if (energia.Count()==0) 
            {
                MessageBox.Show("O Vetor de energias esta vazio ");

            }

        }


        void GravaValores(string nome)
        {

            double minx = Convert.ToDouble(textBox10.Text);
            double maxx = Convert.ToDouble(textBox16.Text);
            double intx = Convert.ToDouble(textBox17.Text);
            int precx = Convert.ToInt32(textBox8.Text);
            double miny = Convert.ToDouble(textBox27.Text);
             double maxy; 
            if (textBox26.Text != "")   maxy = Convert.ToDouble(textBox26.Text); else maxy=0.0;
            double inty = Convert.ToDouble(textBox25.Text);
            int n = Convert.ToInt32(textBox15.Text);
            double kalphastart = Convert.ToDouble(textBox6.Text);
            double kalphaend = Convert.ToDouble(textBox22.Text);
            double kbetastart = Convert.ToDouble(textBox23.Text);
            double kbetaend = Convert.ToDouble(textBox24.Text);
            int numpicos = Convert.ToInt32(textBox1.Text);
            double energmax = Convert.ToDouble(textBox4.Text);
            double proxpico = Convert.ToDouble(textBox7.Text);

            //...



            using (StreamWriter writer = new StreamWriter(nome))
            {

                writer.Write(minx);
                writer.WriteLine();

                writer.Write(maxx);
                writer.WriteLine();

                writer.Write(intx);
                writer.WriteLine();

                writer.Write(precx);
                writer.WriteLine();

                writer.Write(miny);
                writer.WriteLine();

                writer.Write(maxy);
                writer.WriteLine();

                writer.Write(inty);
                writer.WriteLine();

                writer.Write(n);
                writer.WriteLine();

                writer.Write(kalphastart);
                writer.WriteLine();

                writer.Write(kalphaend);
                writer.WriteLine();

                writer.Write(kbetastart);
                writer.WriteLine();

                writer.Write(kbetaend);
                writer.WriteLine();

                writer.Write(numpicos);
                writer.WriteLine();

                writer.Write(energmax);
                writer.WriteLine();

                writer.Write(proxpico);
                writer.WriteLine();

            }

        }

        void CarregaValores()
        {
            string caminho = "C:\\Users\\LR\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\Espessura\\bin\\Release\\origval.out";


            System.IO.StreamReader file = new System.IO.StreamReader(caminho);
            try
            {

                string line;
                line = file.ReadLine();
                textBox10.Text = line; line = file.ReadLine();
                textBox16.Text = line; line = file.ReadLine();
                textBox17.Text = line; line = file.ReadLine();
                textBox8.Text = line; line = file.ReadLine();
                textBox27.Text = line; line = file.ReadLine();
                textBox26.Text = line; line = file.ReadLine();
                textBox25.Text = line; line = file.ReadLine();
                textBox15.Text = line; line = file.ReadLine();
                textBox6.Text = line; line = file.ReadLine();
                textBox22.Text = line; line = file.ReadLine();
                textBox23.Text = line; line = file.ReadLine();
                textBox24.Text = line; line = file.ReadLine();
                textBox1.Text = line; line = file.ReadLine();
                textBox4.Text = line; line = file.ReadLine();
                textBox7.Text = line;
                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
            }

        }


        void CarregaValores(Stream myStream, string nome)
        {
            if (myStream != null)
            {
                using (myStream)
                {
                    /// vou tentar fazer tudo em c#... 
                    /// MessageBox.Show(openFileDialog1.FileName); 
                    //string text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                    nome_arquivo = nome;
                    System.IO.StreamReader file = new System.IO.StreamReader(nome_arquivo);
                    //textBoxCalib.Clear();

                    try
                    {

                        string line;
                        line = file.ReadLine();
                        textBox10.Text = line; line = file.ReadLine();
                        textBox16.Text = line; line = file.ReadLine();
                        textBox17.Text = line; line = file.ReadLine();
                        textBox8.Text = line; line = file.ReadLine();
                        textBox27.Text = line; line = file.ReadLine();
                        textBox26.Text = line; line = file.ReadLine();
                        textBox25.Text = line; line = file.ReadLine();
                        textBox15.Text = line; line = file.ReadLine();
                        textBox6.Text = line; line = file.ReadLine();
                        textBox22.Text = line; line = file.ReadLine();
                        textBox23.Text = line; line = file.ReadLine();
                        textBox24.Text = line; line = file.ReadLine();
                        textBox1.Text = line; line = file.ReadLine();
                        textBox4.Text = line; line = file.ReadLine();
                        textBox7.Text = line;
                        file.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                    }

                }
            }
        }


        private void NormalizaDados(List<double> energia, List<double> contagem , out List<double> xnorm,out List<double> ynorm)
        {
            xnorm = new List<Double>();
            ynorm = new List<Double>();
            xnorm.Clear();
            ynorm.Clear();
                    
            /// normalizando eixo y 
            /// y deve possuir o numero certo de contagens 
            double maxy = contagem.Max();
            double miny = contagem.Min();
            double maxx = energia.Max();
            double minx = energia.Min();
            double temp;
            if (maxx !=0 && maxy!=0)
            {
                for (int i = 0; i < contagem.Count; i++)
                {
                    temp = Math.Abs(contagem.ElementAt(i));
                    ynorm.Add(temp / maxy);
                }



                ///normalizando eixo x 
                ///
                
                for (int i = 0; i < energia.Count; i++)
                {
                   temp =  ( Math.Abs(energia.ElementAt(i)) - minx );
                   xnorm.Add((temp / (maxx - minx)) * maxx);
                }
            }
            

        }

        void Escreve_Nodos(List<int> nodos, List<double> energia, List<double> contagem, string nome)
        {
            //...
            string separator = "\t \t";
            using (StreamWriter writer = new StreamWriter(nome))
            {
                for (int i = 0; i < nodos.Count; i++)
                {
                    int pos;
                    pos = nodos.ElementAt(i); 
                    double en, cont;
                    en = energia.ElementAt(pos);
                    cont = contagem.ElementAt(pos); 
                    writer.Write(en.ToString());
                    writer.Write(separator);
                    writer.WriteLine(cont.ToString());
                }
            }
        }
         private void BuscaNodos()
        {
            /// busca os nodos 
            /// 
           
           int tamcont = contagem_suave.Count;
           
         nodo.Clear();
          
       double ant, atual, prox;
       bool teste1, teste2;
       /// Busca os nodos 
       for (int i = 1; i < (tamcont - 1); i++)
       {


           ant = contagem_suave.ElementAt(i - 1);
           atual = contagem_suave.ElementAt(i);
           prox = contagem_suave.ElementAt(i + 1);

           teste1 = ((atual - ant) < 0);
           teste2 = ((prox - ant) > 0);

           if (teste1 && teste2)      nodo.Add(i);

       }
       
        }

        private void RemoveBG()
        {
              
           // Escreve_arquivo(energia, contagem, "func.out");
           // Escreve_Nodos(nodo, energia, contagem_suave, "nodos.out");
            
            /// Interpola os nodos com Splines cúbicos 
            /// 
            Interp_Splines_Natural_BG(nodo,  contagem_suave, out contagem_suave_sembg);                         
        }


        private void Interp_Splines_Natural_BG2(List<int> nodo, List<double> contagem_suave, out List<double> contagem_suave_sembg)
        {


            List<double> temporario = new List<Double>();

            List<double> dados = new List<Double>();
            List<double> funcao = new List<Double>();

            Interp_nodos = new List<Double>();
            Interp_nodos.Clear();
            contagem_suave_sembg = new List<Double>();
            contagem_suave_sembg.Clear();

            int tamdados = contagem_suave.Count();  /// numero de pontos no vetor energia interpolado 

            int n = nodo.Count();                   /// numero de pontos no vetor energia não interpolado 
            // MessageBox.Show(" N vale = " + n.ToString()); 
            int tam = n - 2;                            ///numero de variaeis que preciso para resolver o sistemaa
            // MessageBox.Show(" tam = " + tam.ToString());
            List<double> x, sol, y, h, lambda, a, b, c, d, lambda2, meio, S, saiday;

            // transferindo os pontos do nodo para o vetor a ser interpolado 
            int pos;
            double func;



            for (int i = 0; i < nodo.Count; i++)
            {
                pos = nodo.ElementAt(i); /// pega a posicao do nodo 
                dados.Add(energia.ElementAt(pos)); /// busca a  energia na posicao do nodo 
                func = contagem_original.ElementAt(pos); /// busca a contagem original na posicao do nodo 
                funcao.Add(func);


            }

            Escreve_arquivo(dados, funcao, "energianodos.txt");

            x = new List<double>(dados);

            y = new List<double>(funcao);

            h = new List<double>(n);

            a = new List<double>(n);
            b = new List<double>(n);
            c = new List<double>(n);
            d = new List<double>(n);

            S = new List<double>(n);


            /// sistema 

            lambda = new List<double>(tam);
            lambda2 = new List<double>(tam);
            meio = new List<double>(tam);
            sol = new List<double>(tam);

            saiday = new List<double>(tamdados);

            // double delta=0.01; // distancia entre pontos no plot
            // double px; // ponto a ser interpolado no plot
            double pt, st, tt, qt; // termos do spline cubico
            int k = 0, j = 0; // contador

            // MessageBox.Show ("Aqui" ); 
            //MessageBox.Show(x.Count.ToString());
            //MessageBox.Show(y.Count.ToString());
            //MessageBox.Show(n.ToString()); 


            



            for (int i = 0; i < (n - 1); i++) // i = 0 ... n-2
            {

                j = i + 1;
                h.Add(x[j] - x[i]); //  h[1] = x[1] - x[0] ... // ok
                // MessageBox.Show(h[i].ToString());
                //cout << i << " " << h[i] << endl;
            }

            for (int i = 0; i < tam; i++) // i = 0 ... n-2
            {
                meio.Add(4.0000);
                lambda.Add(1.0000);
                lambda2.Add(1.0000);

                //cout << lambda [i] << " " << meio [i] << " " << lambda2[i] << endl;
            }

            for (int i = 0; i < tam; i++) // i = 0 ... n-1
            {

                pt = 6.0 / (h.ElementAt(i) * h.ElementAt(i));
                sol.Add(pt * (y.ElementAt(i + 2) - (2 * y.ElementAt(i + 1)) + y.ElementAt(i)));
                //cout << "sol "<<sol[i] << endl ;

            }

            S = Tridiag(lambda, meio, lambda2, sol, tam);

            double[] M = new double[n];

            //List<double> M = new List<double>(n);

            M[0] = 0;
            S.CopyTo(1, M, 1, n - 2);
            M[n - 1] = 0;



            // for (i = 0; i < n; i++)
            {
                ///cout << "M[" << i + 1 << "]=" << setprecision(5) << S[i] << endl;
            }


            //calculando os coeficientes :
            double temp, soma;
            for (int i = 0; i < n - 2; i++)
            {

                a.Add((M[i + 1] - M[i]) / (6.0000 * h[i]));
                b.Add((M[i]) / (2.000));

                temp = ((M[i + 1] + (2.0000 * M[i])) / 6.0000) * h[i];

                c.Add(((y[i + 1] - y[i]) / (h[i])) - temp);
                d.Add(y[i]);
                //cout << a[i] << " " << b[i] << " " << c[i] << " " << d[i] << " " << temp << endl;
                //cout << px -x[0] << endl ;
            }

            Interp_nodos.Clear();

            int l = 0;
            double xima1, xi , xis;
            for (k = 0; k < (n - 2); k++)
            {

                do
                {
                    if (x.ElementAt(0) > energia.ElementAt(l))
                    {
                        pt = 0.0;
                        st = 0.0;
                        tt = 0.0;
                        qt = 0.0;
                    }
                    else
                    {
                        xis = energia.ElementAt(l);
                        xi = x.ElementAt(k);
                        xima1 = x.ElementAt(k+1);

                        pt = Elevado((xima1 - xis),3)/(6.0*h[k+1])*M[k];
                        st = Elevado((xis - xi),3)/(6.0*h[l+1])*M[k+1];
                        tt = (xis - xi)/h[l+1] * (y[l+1]-y[l]) -h[k+1]*h[k+1]/6.0 * (M[k+1]- M[k]) ;
                        qt = y[l] - h[k + 1] * h[k + 1] / (6.0 * M[k]);

                    }
                    soma = pt + st + tt + qt;
                    Interp_nodos.Add(soma < 0 ? 0.0 : soma);


                    l++;
                }
                while (energia.ElementAt(l) <= x.ElementAt(k + 1));

            }
           
            int tamx = x.Count() - 1;
            k = n - 3; /// tratamento do ultimo ponto 
            do
            {
                if (x.ElementAt(tamx) < energia.ElementAt(l))
                {
                    pt = 0.0;
                    st = 0.0;
                    tt = 0.0;
                    qt = 0.0;
                }
                else
                {
                    xis = energia.ElementAt(l);
                    xi = x.ElementAt(k);
                    xima1 = x.ElementAt(k + 1);

                    pt = Elevado((xima1 - xis), 3) / (6.0 * h[k + 1]) * M[k];
                    st = Elevado((xis - xi), 3) / (6.0 * h[l + 1]) * M[k + 1];
                    tt = (xis - xi) / h[l + 1] * (y[l + 1] - y[l]) - h[k + 1] * h[k + 1] / 6.0 * (M[k + 1] - M[k]);
                    qt = y[l] - h[k + 1] * h[k + 1] / (6.0 * M[k]);

                }

                soma = pt + st + tt + qt;

                Interp_nodos.Add(soma < 0 ? 0.0 : soma);
                l++;
            }
            while (l < energia.Count);


            for (int i = 0; i < Interp_nodos.Count; i++)
            {

                contagem_suave_sembg.Add(contagem_original.ElementAt(i) - Interp_nodos.ElementAt(i));

                contagem[i] = contagem_suave_sembg.ElementAt(i);
            }

            //Escreve_arquivo(_saidaenergia, _saidacontagem, nome_arq_saida);

        }

        private void Interp_Splines_Natural_BG(List<int> nodo, List<double> contagem_suave, out List<double> contagem_suave_sembg)
        {

           
            List<double> temporario = new List<Double>();
            
            List<double> dados = new List<Double>();
            List<double> funcao = new List<Double>();

            Interp_nodos = new List<Double>();
            Interp_nodos.Clear();
            contagem_suave_sembg = new List<Double>();
            contagem_suave_sembg.Clear();

            int tamdados = contagem_suave.Count();  /// numero de pontos no vetor energia interpolado 

            int n = nodo.Count();                   /// numero de pontos no vetor energia não interpolado 
            // MessageBox.Show(" N vale = " + n.ToString()); 
            int tam = n - 2;                            ///numero de variaeis que preciso para resolver o sistemaa
            // MessageBox.Show(" tam = " + tam.ToString());
            List<double> x, sol, y, h, lambda, a, b, c, d, lambda2, meio, S, saiday;

            // transferindo os pontos do nodo para o vetor a ser interpolado 
            int pos; 
            double func;


          
            for (int i = 0; i < nodo.Count; i ++  )
            {
                pos = nodo.ElementAt(i); /// pega a posicao do nodo 
                dados.Add(energia.ElementAt(pos)); /// busca a  energia na posicao do nodo 
                func = contagem_original.ElementAt(pos); /// busca a contagem original na posicao do nodo 
                funcao.Add(func);
               
                
            }


           // Escreve_arquivo(dados, funcao, "energianodos.txt");
            x = new List<double>(dados);

            y = new List<double>(funcao);

            h = new List<double>(n);

            a = new List<double>(n);
            b = new List<double>(n);
            c = new List<double>(n);
            d = new List<double>(n);

            S = new List<double>(n);


            /// sistema 

            lambda = new List<double>(tam);
            lambda2 = new List<double>(tam);
            meio = new List<double>(tam);
            sol = new List<double>(tam);

            saiday = new List<double>(tamdados);

            // double delta=0.01; // distancia entre pontos no plot
            // double px; // ponto a ser interpolado no plot
            double pt, st, tt, qt; // termos do spline cubico
            int k = 0,  j = 0; // contador

            // MessageBox.Show ("Aqui" ); 
            //MessageBox.Show(x.Count.ToString());
            //MessageBox.Show(y.Count.ToString());
            //MessageBox.Show(n.ToString()); 

            for (int i = 0; i < (n - 1); i++) // i = 0 ... n-2
            {

                j = i + 1;
                h.Add(x[j] - x[i]); //  h[1] = x[1] - x[0] ... // ok
                // MessageBox.Show(h[i].ToString());
                //cout << i << " " << h[i] << endl;
            }

            for (int i = 0; i < tam; i++) // i = 0 ... n-2
            {
                meio.Add(4.0000);
                lambda.Add(1.0000);
                lambda2.Add(1.0000);

                //cout << lambda [i] << " " << meio [i] << " " << lambda2[i] << endl;
            }

            for (int i = 0; i < tam; i++) // i = 0 ... n-1
            {

                pt = 6.0 / (h.ElementAt(i) * h.ElementAt(i));
                sol.Add(pt * (y.ElementAt(i + 2) - (2 * y.ElementAt(i + 1)) + y.ElementAt(i)));
                //cout << "sol "<<sol[i] << endl ;

            }

            S = Tridiag(lambda, meio, lambda2, sol, tam);

            double[] M = new double[n];

            //List<double> M = new List<double>(n);

            M[0] = 0;
            S.CopyTo(1, M, 1, n - 2);
            M[n - 1] = 0;



            // for (i = 0; i < n; i++)
            {
                ///cout << "M[" << i + 1 << "]=" << setprecision(5) << S[i] << endl;
            }


            //calculando os coeficientes :
            double temp, soma ;
            for (int i = 0; i < n - 2; i++)
            {

                a.Add((M[i + 1] - M[i]) / (6.0000 * h[i]));
                b.Add((M[i]) / (2.000));

                temp = ((M[i + 1] + (2.0000 * M[i])) / 6.0000) * h[i];

                c.Add(((y[i + 1] - y[i]) / (h[i])) - temp);
                d.Add(y[i]);
                //cout << a[i] << " " << b[i] << " " << c[i] << " " << d[i] << " " << temp << endl;
                //cout << px -x[0] << endl ;
            }

            Interp_nodos.Clear();

            int l = 0;
            for (k = 0; k < (n - 2); k++)
            {

                do
                {
                    if (x.ElementAt(0) > energia.ElementAt(l))
                    {
                        pt = 0.0;
                        st = 0.0;
                        tt = 0.0;
                        qt = 0.0; 
                    }
                    else 
                    {
                        pt = (a.ElementAt(k) * Elevado((energia.ElementAt(l) - x.ElementAt(k)), 3));
                        st = (b.ElementAt(k) * Elevado((energia.ElementAt(l) - x.ElementAt(k)), 2));
                        tt = (c.ElementAt(k) * (energia.ElementAt(l) - x.ElementAt(k)));
                        qt = (d.ElementAt(k));
                    }
                    soma = pt + st + tt + qt;
                    Interp_nodos.Add(soma < 0 ? 0.0 : soma);


                    l++;
                }
                while (energia.ElementAt(l) <= x.ElementAt(k + 1));

            }

            int tamx = x.Count() - 1; 
            k = n - 3; /// tratamento do ultimo ponto 
            do
            {
                if (x.ElementAt(tamx) < energia.ElementAt(l))
                {
                    pt = 0.0;
                    st = 0.0;
                    tt = 0.0;
                    qt = 0.0;
                }
                else 
                {
                    pt = (a.ElementAt(k) * Elevado((energia.ElementAt(l) - x.ElementAt(k)), 3));
                    st = (b.ElementAt(k) * Elevado((energia.ElementAt(l) - x.ElementAt(k)), 2));
                    tt = (c.ElementAt(k) * (energia.ElementAt(l) - x.ElementAt(k)));
                    qt = (d.ElementAt(k));

                }
                
                soma = pt + st + tt + qt;
                
                Interp_nodos.Add(soma<0?0.0:soma);
                l++;
            }
            while (l < energia.Count);

          
            for (int i = 0 ; i < Interp_nodos.Count; i ++ )
            {
                double sembg = contagem_original.ElementAt(i) - Interp_nodos.ElementAt(i) ;

                if (sembg < 0.0)  
                    contagem_suave_sembg.Add(0.0);   
                else
                    contagem_suave_sembg.Add(contagem_original.ElementAt(i) - Interp_nodos.ElementAt(i)); 
              
                contagem[i] = contagem_suave_sembg.ElementAt(i); 
            }

          //  Escreve_arquivo(energia, Interp_nodos, "teste_spline.txt");

        }



        private void Suaviza(List<double> contagem, int janela)
        {
            
            int l;
            double sum = 0.0;
            contagem_suave.Clear(); 
             /// copia os janela primeiros 
            for (int i = 0; i < (janela); i++)
            {
                contagem_suave.Add(contagem.ElementAt(i));

            }

            /// suaviza os do meio 
            for (int i = (janela); i < (contagem.Count-janela); i++)
            {
                sum =0.0;
                for (int j = -janela; j <= janela; j++)
                {
                    sum += contagem.ElementAt(j+i);
                }
                contagem_suave.Add(1.0 / (2 * janela + 1) * sum);
            }


            int tamcontagem = contagem.Count()-1;
            /// copia os janela ultimos 
            for (int i = janela+1; i > 0; i--)
            {
                contagem_suave.Add(contagem.ElementAt(tamcontagem - i));

            }

            /// passa os dados para contagem (só para testes ) 
            for (int i = 0; i <tamcontagem; i++)
            {
                contagem[i] = contagem_suave.ElementAt(i);
            }
            


        }
        private void RetornaEnxCont()
        {
            energia.Clear();
            contagem.Clear();

            for (int i = 0; i < energia_original.Count; i ++ )
            {
                energia.Add( energia_original.ElementAt(i));
                contagem.Add( contagem_original.ElementAt(i));
            }
        }

        /// <summary>
        /// Clique no botao Gera grafico 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {

            LimpaTudo();

            RetornaEnxCont();  /// retorna o valor original aos vetores energia e contagem 

            button5.Enabled = true; 
           

            textBox28.Text = nome_arquivo;
            int continuar = 0; 
            /// se houver arquivo aberto 
            if (contagem.Count > 1)
            {



                string nome = "C:\\Users\\LR\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\Espessura\\bin\\Release\\origval.out";                      /// 
                GravaValores(nome); /// salva as configurações utilizadas 

                if (comboBox2.SelectedIndex != (-1)) // se escolheu algum método de interpolacao válido , Interpole
                 {

                        progressBar1.Value = 0;

                        //if (comboBox2.SelectedIndex != 3)


                        if (checkBox8.Checked)
                        {
                            Suaviza(contagem, Convert.ToInt32(textBox13.Text));
                            BuscaNodos();
                            RemoveBG();
                            
                        }


                        continuar =  Calcula_regiao_Ka_Kb();
                        if (continuar == 0)
                        {
                            

                            Interpola(comboBox2.SelectedIndex);
                            progressBar1.Value = 60;

                            //Escreve_Comandos_Gnuplot();
                            GeraGrafico(1);

                            progressBar1.Value = 100;

                        }else
                        {

                            GeraGrafico(-1);
                        }
                        

                            

                        //ChamaGnuplot();


                        //string path = Directory.GetCurrentDirectory();
                        //pictureBox2.LoadAsync(path + "\\saida.jpg");
                   
                   

                }
                else /// sem interpolacao 
                {

                  

                    if (checkBox8.Checked)
                    {
                        Suaviza(contagem, Convert.ToInt32(textBox13.Text));
                        BuscaNodos();
                        RemoveBG();

                    }

                    continuar=Calcula_regiao_Ka_Kb();
                    if (continuar == 0)
                    {
                        GeraGrafico(0);
                    }
                    else
                    {
                        //MessageBox.Show("Erro ");
                        GeraGrafico(-1);
                    }

                    


                }

                if (checkBox7.Checked)   /// Imprime os dados sem interpolacao 
                {
                    List<double> xnorm  ;
                    List<double> ynorm ;
                    NormalizaDados(energia,contagem,out xnorm,out ynorm); 
                    Escreve_arquivo(xnorm, ynorm, "dadosseminterpnorm.dat");
                    xnorm.Clear();
                    ynorm.Clear();
                    
                }
            }


            else
            {
                MessageBox.Show("Erro: Carregue um arquivo de dados para leitura !  ");

            }



        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

      






        /// <summary>
        ///  Algoritmo de detecção de Picos DPMCA
        /// </summary>
        /// <param name="num_picos_manter"></param>
        /// <param name="energia_maxima"></param>
        /// <param name="_vec_energia"></param>
        /// <param name="_vec_contagem"></param>
        void DetectaPicos_DPMCA(int num_picos_manter, double energia_maxima, List<double> _vec_energia, List<double> _vec_contagem)
        {
            ///verificar se há grafico carregado , senão mostrar message box 
            ///
            /// percorrer o array buscando mudancas de concavidades 
            /// 


            double maior;
            int posmaior;

            var energiaxcontagemxposen = new List<KeyValuePair<KeyValuePair<double, double>, int>>();

            ////////////////////////////////////////////////////
            // energiaxcontagemxposen = vetor onde eu guardo os picos 
            // energiaxcontagemxposen.Key = o par "energia,contagem"
            // energiaxcontagemxposen.Key.Key = energia 
            // energiaxcontagemxposen.Key.Value = Contagem 
            // energiaxcontagemxposen.Value = Posicao do pico no vetor energia ou contagem ( mesmo indice)   
            //////////////////////////////////////////////////////

            int cont_pair = 0;
            maior = 0.0;
            /// Algoritmo que busca picos 
            /// 
            maior = _vec_contagem.Max();
            posmaior = _vec_contagem.IndexOf(maior); /// acha a posicao do maior 
            //MessageBox.Show(" Pos MAIOR " + _vec_energia.ElementAt(posmaior) + " " +_vec_contagem.ElementAt(posmaior) ); 


            /// pico 
            /// 
            /// par energia X contagem
            var par = new KeyValuePair<double, double>(_vec_energia.ElementAt(posmaior), _vec_contagem.ElementAt(posmaior));
            /// par (energiaXcontagem) vs posicao no vetores 
            var par_par = new KeyValuePair<KeyValuePair<double, double>, int>(par, posmaior);


            energiaxcontagemxposen.Add(par_par);
            /// posicao no vetor energia 

            cont_pair++; /// conta quantos pares foram adicionados 




            /// fim algoritmo picos 
            /// 

            int pos_en_prim_esq = 0,      /// posicao dentro do vetor _vec_energia do primeiro ponto com contagens menores
                /// que metade do pico a esquerda

                  pos_en_prim_dir = 0;   /// posicao dentro do vetor _vec_energia doprimeiro ponto com contagens menores
            /// que metade do pico a direita

            double en_prim_esq = 0.0,    /// energia do primeiro ponto com contagens menores
                /// que metade do pico a esquerda

                   en_prim_dir = 0.0;   /// energia do primeiro ponto com contagens menores
            /// que metade do pico a direita

            // organizar o vetor do menor para o maior 
            energiaxcontagemxposen.Sort(Compare_Pela_Contagem);

            /// organizar o vetor do maior para o menor 
            energiaxcontagemxposen.Reverse();


            int cont = 0; /// utilizado para saber quantos picos eu mantenho no meu richtextbox
            int posenergia = 0;   /// variavel temporaria 

           
           

            foreach (var pair in energiaxcontagemxposen)
            {

                if (cont < num_picos_manter && pair.Key.Key < energia_maxima)
                {


                    /// posicao dentro do vetor _vec_energia 
                    posenergia = pair.Value;

                    /// calcula a largura a meia altura do pico 
                    /// retorna as posicoes no vetor _vec_energia, que leva nas primeiras posicoes 
                    /// no vetor _vec_contagem a direita e a esquerda, menores que metade das contagens do pico
                    Calcula_Largura_Meia_Altura(posenergia, pair.Key.Value, out pos_en_prim_esq, out pos_en_prim_dir, _vec_contagem);

                    if (pos_en_prim_dir == -1 || pos_en_prim_esq == -1)
                    {
                        //MessageBox.Show(" Nao encontrou o valor a esquerda ou direita refaça a interpolacao");

                        // MessageBox.Show(pos_en_prim_dir.ToString() + " " + pos_en_prim_esq.ToString());
                    }
                    else
                    {
                        en_prim_esq = _vec_energia.ElementAt(pos_en_prim_esq);
                        en_prim_dir = _vec_energia.ElementAt(pos_en_prim_dir);

                        // MessageBox.Show ( posenergia.ToString() + " " + pair.Key.Value + " " + en_prim_esq.ToString() + " " + en_prim_dir.ToString() );
                        richTextBox1.AppendText(" Energia: " + pair.Key.Key + " Contagem : " + pair.Key.Value.ToString());
                        richTextBox1.AppendText(System.Environment.NewLine);
                        // adicione as informacoes 
                        richTextBox1.AppendText(" Contagem ideal para LMA  =  " + (pair.Key.Value / 2.0).ToString());
                        richTextBox1.AppendText(System.Environment.NewLine);
                        //richTextBox1.AppendText(" esq = " + (en_prim_esq).ToString() + "  contagem : " + (_vec_contagem.ElementAt(pos_en_prim_esq)).ToString());
                        //richTextBox1.AppendText(System.Environment.NewLine);
                        //richTextBox1.AppendText(" dir = " + (en_prim_dir).ToString() + "  contagem : " + (_vec_contagem.ElementAt(pos_en_prim_dir)).ToString());
                        richTextBox1.AppendText(System.Environment.NewLine);
                        richTextBox1.AppendText(System.Environment.NewLine);

                        /// verificar se o pico já esta contido na colecao 
                        /// 



                        /// faz o par contagem menor a esquerda e contagem menor a direita 
                        var par_double = new KeyValuePair<double, double>(en_prim_esq, en_prim_dir);
                        /// faz o par pico ( maior contagem) , par_double ( contagem menor a esquerda e contagem menor a direita )
                        var par_double2 = new KeyValuePair<double, KeyValuePair<double, double>>(pair.Key.Value, par_double);
                        picos.Add(par_double2);
                        cont++;
                    }

                }
            }
        }















        /// <summary>
        ///  Algoritmo de detecção de Picos . Determina LMA 
        /// </summary>
        /// <param name="num_picos_manter"></param>
        /// <param name="energia_maxima"></param>
        /// <param name="_vec_energia"></param>
        /// <param name="_vec_contagem"></param>
        void DetectaPicos(int num_picos_manter, double energia_maxima, List<double> _vec_energia, List<double> _vec_contagem)
        {
            ///verificar se há grafico carregado , senão mostrar message box 
            ///
            /// percorrer o array buscando mudancas de concavidades 
            /// 

            double delta_antes, delta_depois, antes, meio, depois;

            var energiaxcontagemxposen = new List<KeyValuePair<KeyValuePair<double, double>, int>>();

            ////////////////////////////////////////////////////
            // energiaxcontagemxposen = vetor onde eu guardo os picos 
            // energiaxcontagemxposen.Key = o par "energia,contagem"
            // energiaxcontagemxposen.Key.Key = energia 
            // energiaxcontagemxposen.Key.Value = Contagem 
            // energiaxcontagemxposen.Value = Posicao do pico no vetor energia ou contagem ( mesmo indice)   
            //////////////////////////////////////////////////////

            int cont_pair = 0;

            /// Algoritmo que busca picos 
            for (int i = 1; i < (_vec_contagem.Count - 1); i++) /// do primeiro elemento ao penúltimo 
            {
                antes = Math.Abs(_vec_contagem.ElementAt(i - 1));
                meio = Math.Abs(_vec_contagem.ElementAt(i));
                depois = Math.Abs(_vec_contagem.ElementAt(i + 1));

                delta_antes = (meio - antes);
                delta_depois = (depois - meio);
                if ((delta_antes > 0 && delta_depois < 0)  )
                {
                    /// pico 
                    /// 
                    /// par energia X contagem
                    var par = new KeyValuePair<double, double>(_vec_energia.ElementAt(i), _vec_contagem.ElementAt(i));
                    /// par (energiaXcontagem) vs posicao no vetores 
                    var par_par = new KeyValuePair<KeyValuePair<double, double>, int>(par, i);
                    energiaxcontagemxposen.Add(par_par);
                    /// posicao no vetor energia 
                    //MessageBox.Show(_vec_energia.ElementAt(i).ToString() + " " + _vec_contagem.ElementAt(i).ToString() );
                    cont_pair++; /// conta quantos pares foram adicionados 
                }


            }

            /// descartar os picos com menor contagem relativa 
            /// 
            /// 
            /// 
            /// 
            
            /// fim algoritmo picos 
            /// 

            int pos_en_prim_esq = 0,      /// posicao dentro do vetor _vec_energia do primeiro ponto com contagens menores
                /// que metade do pico a esquerda

                  pos_en_prim_dir = 0;   /// posicao dentro do vetor _vec_energia doprimeiro ponto com contagens menores
            /// que metade do pico a direita

            double en_prim_esq = 0.0,    /// energia do primeiro ponto com contagens menores
                /// que metade do pico a esquerda

                   en_prim_dir = 0.0;   /// energia do primeiro ponto com contagens menores
            /// que metade do pico a direita

            // organizar o vetor do menor para o maior 
            energiaxcontagemxposen.Sort(Compare_Pela_Contagem);

            /// organizar o vetor do maior para o menor 
            energiaxcontagemxposen.Reverse();


            int cont = 0; /// utilizado para saber quantos picos eu mantenho no meu richtextbox
            int posenergia = 0;   /// variavel temporaria 


            foreach (var pair in energiaxcontagemxposen)
            {
                if (cont < num_picos_manter && pair.Key.Key < energia_maxima)
                {


                    /// posicao dentro do vetor _vec_energia 
                    posenergia = pair.Value;

                    /// calcula a largura a meia altura do pico 
                    /// retorna as posicoes no vetor _vec_energia, que leva nas primeiras posicoes 
                    /// no vetor _vec_contagem a direita e a esquerda, menores que metade das contagens do pico
                    Calcula_Largura_Meia_Altura(posenergia, pair.Key.Value, out pos_en_prim_esq, out pos_en_prim_dir, _vec_contagem);

                    if (pos_en_prim_dir == -1 || pos_en_prim_esq == -1)
                    {
                      //  MessageBox.Show(" Nao encontrou o valor a esquerda ou direita refaça a interpolacao");

                      //  MessageBox.Show(pos_en_prim_dir.ToString() + " " + pos_en_prim_esq.ToString());
                    }
                    else
                    {
                        en_prim_esq = _vec_energia.ElementAt(pos_en_prim_esq);
                        en_prim_dir = _vec_energia.ElementAt(pos_en_prim_dir);

                        /// faz o par contagem menor a esquerda e contagem menor a direita 
                        var par_double = new KeyValuePair<double, double>(en_prim_esq, en_prim_dir);
                        /// faz o par pico ( maior contagem) , par_double ( contagem menor a esquerda e contagem menor a direita )
                        var par_double2 = new KeyValuePair<double, KeyValuePair<double, double>>(pair.Key.Value, par_double);
                        picos.Add(par_double2);

                        // MessageBox.Show ( posenergia.ToString() + " " + pair.Key.Value + " " + en_prim_esq.ToString() + " " + en_prim_dir.ToString() );
                        richTextBox1.AppendText(" Energia: " + pair.Key.Key + " Contagem : " + pair.Key.Value.ToString());
                        richTextBox1.AppendText(System.Environment.NewLine);
                        // adicione as informacoes 
                        richTextBox1.AppendText(" Contagem ideal para LMA  =  " + (pair.Key.Value / 2.0).ToString());
                        richTextBox1.AppendText(System.Environment.NewLine);
                        //richTextBox1.AppendText(" esq = " + (en_prim_esq).ToString() + "  contagem : " + (_vec_contagem.ElementAt(pos_en_prim_esq)).ToString());
                        //richTextBox1.AppendText(System.Environment.NewLine);
                        //richTextBox1.AppendText(" dir = " + (en_prim_dir).ToString() + "  contagem : " + (_vec_contagem.ElementAt(pos_en_prim_dir)).ToString());
                        richTextBox1.AppendText(System.Environment.NewLine);
                        richTextBox1.AppendText(System.Environment.NewLine);


                        cont++;
                    }

                }
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            button5.Enabled = true; 
            Int16 num_manter = Convert.ToInt16(textBox1.Text);
            double energia_maxima = StrtoDbl(textBox4.Text); /// acima desta energia , descartar 



            if (textBox1.Text.Length != 0) /// se existe um numero de picos a manter 
            {

                if (checkBox2.Checked == true) /// usa dados interpolados 
                {

                    if (opcao_programa == 1)  // DPMCA 
                    {

                        richTextBox1.Clear();  /// limpe o texto 
                        /// 
                        picos.Clear(); /// zera o vetor dos picos 
                        /// 
                        richTextBox1.AppendText(" Dados Kalpha ");  /// limpe o texto 
                        richTextBox1.AppendText(System.Environment.NewLine);
                        DetectaPicos_DPMCA(num_manter, energia_maxima, kalpha_interp_energia, kalpha_interp_cont);
                        richTextBox1.AppendText(" Dados KBeta ");  /// limpe o texto 
                        richTextBox1.AppendText(System.Environment.NewLine);
                        DetectaPicos_DPMCA(num_manter, energia_maxima, kbeta_interp_energia, kbeta_interp_cont);

                    }

                    if (opcao_programa == 2 || opcao_programa == 3)  // GEANT  e XRMC
                    {

                        richTextBox1.Clear();  /// limpe o texto 
                        /// 
                        picos.Clear(); /// zera o vetor dos picos 
                        /// 
                        richTextBox1.AppendText(" Dados Kalpha ");  /// limpe o texto 
                        richTextBox1.AppendText(System.Environment.NewLine);
                        DetectaPicos(num_manter, energia_maxima, kalpha_interp_energia, kalpha_interp_cont);
                        richTextBox1.AppendText(" Dados KBeta ");  /// limpe o texto 
                        richTextBox1.AppendText(System.Environment.NewLine);
                        DetectaPicos(num_manter, energia_maxima, kbeta_interp_energia, kbeta_interp_cont);
                    }



                }
                else // usa dados do vetor energia e contagem 
                {
                    if (opcao_programa == 1)  // DPMCA 
                    {

                        richTextBox1.Clear();  /// limpe o texto 
                        /// 
                        picos.Clear(); /// zera o vetor dos picos 
                        /// 
                        richTextBox1.AppendText(" Dados Kalpha ");  /// limpe o texto 
                        richTextBox1.AppendText(System.Environment.NewLine);
                        DetectaPicos_DPMCA(num_manter, energia_maxima, kalpha_energia, kalpha_contagem);
                        richTextBox1.AppendText(" Dados KBeta ");  /// limpe o texto 
                        richTextBox1.AppendText(System.Environment.NewLine);
                        DetectaPicos_DPMCA(num_manter, energia_maxima, kbeta_energia, kbeta_contagem);

                    }

                    if (opcao_programa == 2)  // GEANT 
                    {

                        richTextBox1.Clear();  /// limpe o texto 
                        /// 
                        picos.Clear(); /// zera o vetor dos picos 
                        /// 
                        richTextBox1.AppendText(" Dados Kalpha ");  /// limpe o texto 
                        richTextBox1.AppendText(System.Environment.NewLine);
                        DetectaPicos(num_manter, energia_maxima, kalpha_energia, kalpha_contagem);
                        richTextBox1.AppendText(" Dados KBeta ");  /// limpe o texto 
                        richTextBox1.AppendText(System.Environment.NewLine);
                        DetectaPicos(num_manter, energia_maxima, kbeta_energia, kbeta_contagem);
                    }


                }

                /// atualiza os textbox com os novos picos 
                numericUpDown1_ValueChanged(sender, e);
                numericUpDown2_ValueChanged(sender, e);


            }
            else
            {

                MessageBox.Show("Entre com o numero de picos a manter ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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



        /*
         
            
           
          

        
            double Area = 0;

            double sumpares=0.0 , sumimpares = 0.0 ;

            for ( int j = posmin+1 ; j < posmax  ; j++ )   /// pares
            {
                if (j%2==0)
                    sumpares+=_cont[j];
            }

            for (int j = posmin+1; j < posmax; j++)    ///  impares
            {
                if (j%2!=0)
                    sumimpares  += _cont[j];
            }

            posmax =EncontraPosicao(_cont , en_max*en_max) ;
            posmin =EncontraPosicao(_cont , en_min*en_min) ;


            cout << (h) << " " << _cont[posmin] << " " << sumpares << " " << sumimpares  << " " << _cont[posmax] << endl ;
            Area = (h / 3.0) * (_cont[posmin] + 2 * (sumpares) + 4 * sumimpares + _cont[posmax]);
            /// calcular o erro tb formula no wikipédia
            return Area;
         
         
         
         */


        /// <summary>
        /// / PAG 162 FAIRES E BURDEN 
        /// </summary>
        /// <param name="_en"></param>
        /// <param name="_cont"></param>
        /// <param name="pos_en_min"></param>
        /// <param name="pos_en_max"></param>
        /// <returns></returns>
        private double Simpson_Composta(List<double> _en, List<double> _cont, int pos_en_min, int pos_en_max)
        {




            double h = _en[1] - _en[0]; /// espacamento igual
            double n = pos_en_max - pos_en_min; /// quantos intervalos existem entre um e outro

            //MessageBox.Show(pos_en_max + " " + _en.ElementAt(pos_en_max));
            //MessageBox.Show(pos_en_min + " " + _en.ElementAt(pos_en_min));


            //if (_en.Count != _cont.Count)
            {
              //  MessageBox.Show("tamanhos diferentes de contagem " + _en.Count + " " + _cont.Count);
               // return -1;  /// os vetores nao podem ter tamanhos diferentes 
            }

            //MessageBox.Show(pos_en_min.ToString());
            //MessageBox.Show(pos_en_max.ToString()); 
            double a = _en.ElementAt(pos_en_min),
                    b = _en.ElementAt(pos_en_max);


            if ((n % 2) != 0)
            {
                // MessageBox.Show("Erro , n não é par!"); 

            }


            double Area = 0;

            double sumpares = 0.0, sumimpares = 0.0;
            int cont = 1;
            for (int j = pos_en_min + 1; j < pos_en_max; j++)   /// pares
            {
                
                if ( (cont % 2) == 0)
                    sumpares += 2.0*_cont[j];
                else
                    sumimpares += 4.0 *_cont[j];

                cont++;

            }

           

           // MessageBox.Show(" simpson " + (h / 3.0) + " " + h.ToString() + "   " + (_cont.ElementAt(pos_en_min) + 2 * (sumpares) + 4 * sumimpares + _cont.ElementAt(pos_en_max)).ToString());

            Area = (h / 3.0) * (_cont.ElementAt(pos_en_min) + sumimpares +  sumpares + _cont.ElementAt(pos_en_max));

            //Area = (h / 2.0) * (_cont.ElementAt(pos_en_min) + 2 * (sumpares) + 4 * sumimpares + _cont.ElementAt(pos_en_max)); 
            /// calcular o erro tb formula no wikipédia 
            return Area;

        }


        /// <summary>
        ///  Aproxima por um polinomio de grau 6 , Dependendo de quantos pontos sobram escolhe uma 
        ///  FOrmula de Newton Cottes para realizar , segundo a tabela 7.1 pag 251 numerical Methods 
        /// </summary>
        /// <param name="_en"></param>
        /// <param name="_cont"></param>
        /// <param name="pos_en_min"></param>
        /// <param name="pos_en_max"></param>
        /// <returns></returns>
        private double Weedle_corrigido(List<double> _en, List<double> _cont, int pos_en_min, int pos_en_max)
        {


            double h = _en[1] - _en[0]; /// espacamento igual
            double n = pos_en_max - pos_en_min; /// quantos intervalos existem entre um e outro

            int intervalos = Convert.ToInt32(Math.Floor( n / 7.0) ) ;  /// quantos intervalos eu tenho 
            int sobra = (intervalos % 7);  /// quantos intervalos sobram  
            
           


            //MessageBox.Show(pos_en_max + " " + _en.ElementAt(pos_en_max));
            //MessageBox.Show(pos_en_min + " " + _en.ElementAt(pos_en_min));


           // if (_en.Count != _cont.Count)
            {
             //   MessageBox.Show("tamanhos diferentes de contagem " + _en.Count + " " + _cont.Count);
              //  return -1;  /// os vetores nao podem ter tamanhos diferentes 
            }

            //MessageBox.Show(pos_en_min.ToString());
            //MessageBox.Show(pos_en_max.ToString()); 
            double a = _en.ElementAt(pos_en_min),
                    b = _en.ElementAt(pos_en_max);


            if ((n % 2) != 0)
            {
                // MessageBox.Show("Erro , n não é par!"); 

            }


            double Area = 0;

            double sum= 0.0;
            double valsobra = 0.0;

            int j = pos_en_min;
            if (sobra == 0)
            {
                
                while ((j + 7) <= pos_en_max)
                {
                    if (j+7!= pos_en_max)
                        sum += ((_cont.ElementAt(j) + 5 * _cont.ElementAt(j + 1) + _cont.ElementAt(j + 2) + 6 * _cont.ElementAt(j + 3) + _cont.ElementAt(j + 4) + 5 * _cont.ElementAt(j + 5) + 2 * _cont.ElementAt(j + 6)));
                    else
                        sum += ((_cont.ElementAt(j) + 5 * _cont.ElementAt(j + 1) + _cont.ElementAt(j + 2) + 6 * _cont.ElementAt(j + 3) + _cont.ElementAt(j + 4) + 5 * _cont.ElementAt(j + 5) +  _cont.ElementAt(j + 6)));
                    j += 7;
                }
                sum *= (3.0 * h / 10.0); 
            }
            else
            {

                
                while ((j + 7) < pos_en_max)
                {
                    sum += ((_cont.ElementAt(j) + 5 * _cont.ElementAt(j + 1) + _cont.ElementAt(j + 2) + 6 * _cont.ElementAt(j + 3) + _cont.ElementAt(j + 4) + 5 * _cont.ElementAt(j + 5) + 2 * _cont.ElementAt(j + 6)));
                    j += 7;
                    //MessageBox.Show(sum.ToString());
                }
                sum *= (3.0 * h / 10.0); 

                for (int i=j ; i<=pos_en_max;i++) 
                {
                    valsobra += (_cont.ElementAt(i) * (h));
                }
                
                sum += valsobra;
               
            }
            //MessageBox.Show(n.ToString() + " " + intervalos.ToString() + " " + sobra.ToString() + " " + valsobra.ToString() + " " + sum.ToString() + " " + h.ToString() );
           
            /*
          
            double valsobra = 0.0;

            if (sobra == 1) // repete o valor final 
            {
                valsobra+= (h* _cont.ElementAt(pos_en_max)); 

            }
            if (sobra == 2 ) // trapezio 
            {
                valsobra +=( h / 2.0) * (_cont.ElementAt(pos_en_max) + _cont.ElementAt(pos_en_max-1)); 
            }
            if (sobra == 3) // Simpsons 1/3 
            {
                valsobra += (h / 3.0 )* (_cont.ElementAt(pos_en_max) + 4 * _cont.ElementAt(pos_en_max - 1) + _cont.ElementAt(pos_en_max - 2));
            }
            
            if (sobra == 4) // Simpsons 3/8 
            {
                valsobra += (3*h / 8.0) * (_cont.ElementAt(pos_en_max) + 3 * _cont.ElementAt(pos_en_max - 1) + 3*_cont.ElementAt(pos_en_max - 2) + _cont.ElementAt(pos_en_max - 3));
            }

            if (sobra == 5) // Boole
            {
                valsobra += (2 * h / 45.0 )* (7 * _cont.ElementAt(pos_en_max) + 32 * _cont.ElementAt(pos_en_max - 1) + 12 * _cont.ElementAt(pos_en_max - 2) + 32 * _cont.ElementAt(pos_en_max - 3) + 7 * _cont.ElementAt(pos_en_max - 4));
            }
           sum += valsobra;
           
                

           //MessageBox.Show(n.ToString() + " " + intervalos.ToString() + " " + sobra.ToString() + " " + valsobra.ToString()  + " " + sum.ToString());
           

            Area = sum;

            //MessageBox.Show(" weddle " + (3.0 * h / 10.0) + " " + h.ToString() + "   " + sum.ToString() + "   " + valsobra.ToString());
            
            
            */
            //Area = (h / 2.0) * (_cont.ElementAt(pos_en_min) + 2 * (sumpares) + 4 * sumimpares + _cont.ElementAt(pos_en_max)); 
            /// calcular o erro tb formula no wikipédia 
            return Area;

        }
        private void RecalculaRazao()
        {


            if (checkBox2.Checked == false) /// usar dados sem interpolacao 
            {

                double sumkalpha = 0.0, sumkbeta = 0.0;

                int posmenor = pos_en_kalpha_min;
                int posmaior = pos_en_kalpha_max;
                

                /// Area calculada por soma das contagens 
                /// base * altura ( onde base é a subtracao entre as energias a esquerda e direita e a altura é o numero de contagens do pico 
                /// 

                /// só receber valores que existam no vetor ou truncar para o próximo que exista 
                for (int i = posmenor; i < posmaior; i++)
                {
                    if (contagem.ElementAt(i) > 0.0) sumkalpha += contagem.ElementAt(i);

                }

                posmenor = pos_en_kbeta_min;
                posmaior = pos_en_kbeta_max;

                /// só receber valores que existam no vetor ou truncar para o próximo que exista 
                for (int i = posmenor; i < posmaior; i++)
                {
                    if (contagem.ElementAt(i) > 0.0) sumkbeta += contagem.ElementAt(i);
                }

                double AreaKalpha = sumkalpha;
                double AreaKbeta = sumkbeta;
                largurakalpha = (StrtoDbl(textBox19.Text) - StrtoDbl(textBox18.Text)); /// largura é a diferença entre as energias
                largurakbeta = (StrtoDbl(textBox21.Text) - StrtoDbl(textBox20.Text)); /// largura é a diferença entre as energias

                textBox40.Text = AreaKalpha.ToString();
                textBox41.Text = AreaKbeta.ToString();

           

                textBox35.Text = ((sumkalpha / largurakalpha) / (sumkbeta / largurakbeta)).ToString();





                ////////////////////////////////
                ///  BOX 1 Sem interpolacao 
                /// calcula a area usando metodo de  Simpson entre as larguras a meia altura 
                /// ///////////////////////
                AreaKalpha = Simpson_Composta(energia, contagem, pos_en_kalpha_min, pos_en_kalpha_max);
                AreaKbeta = Simpson_Composta(energia, contagem, pos_en_kbeta_min, pos_en_kbeta_max);

                textBox33.Text = AreaKalpha.ToString();
                textBox34.Text = AreaKbeta.ToString();
                textBox3.Text = (AreaKalpha / AreaKbeta).ToString();


              


                ///  BOX 3 Sem interpolacao 
                /// Area calculada pela metodologia do zani : 
                /// base * altura ( onde base é a subtracao entre as energias a esquerda e direita e a altura é o numero de contagens do pico 
                /// 

                int pospico = Convert.ToInt32(numericUpDown1.Value - 1);
                double pico = picos.ElementAt(pospico).Key;  /// pega o pico selecionado pelo numericupdown
                double alturakalpha = pico;  /// chama o pico de altura 
                largurakalpha = (StrtoDbl(textBox19.Text) - StrtoDbl(textBox18.Text)); /// largura é a diferença entre as energias

                pospico = Convert.ToInt32(numericUpDown2.Value - 1);
                pico = picos.ElementAt(pospico).Key;  /// pega o pico selecionado pelo numericupdown
                double alturakbeta = pico;           /// chama o pico de altura 
                largurakbeta = (StrtoDbl(textBox21.Text) - StrtoDbl(textBox20.Text)); /// largura é a diferença entre as energias

                textBox37.Text = (alturakalpha * largurakalpha).ToString();
                textBox38.Text = (alturakbeta * largurakbeta).ToString();

                textBox36.Text = ((alturakalpha * largurakalpha) / (alturakbeta * largurakbeta)).ToString();


            }



            if (checkBox2.Checked == true) /// usar dados interpolados 
            {

               
                ////////////////////////////////
                //// BOX 1 Com interpolacao  Simpson
                /// calcula a area usando metodo de  Simpson entre as larguras a meia altura 
                ///  /// 1 box
                double AreaKalpha = Simpson_Composta(kalpha_interp_energia, kalpha_interp_cont, pos_en_kalpha_min, pos_en_kalpha_max);
                double AreaKbeta = Simpson_Composta(kbeta_interp_energia, kbeta_interp_cont, pos_en_kbeta_min, pos_en_kbeta_max);

                textBox33.Text = AreaKalpha.ToString();
                textBox34.Text = AreaKbeta.ToString();
                textBox3.Text = (AreaKalpha / AreaKbeta).ToString();



                //////////////////////////////////////////////
                ////  BOX 2 Com interpolacao  Riemann LMA  
                /// calcula a área por somatório das contagens entre os valores de largura a meia altura  
                /// multiplicada pela metodo 2 Riemann LMA  
                /// //////////////////////////////////////////////


                /// kalpha 
                double sumkalpha = 0.0, sumkbeta = 0.0;


                int posmenor = pos_en_kalpha_min;
                int posmaior = pos_en_kalpha_max;

    
                if (posmenor != -1 && posmaior != -1)  /// verificando se encontrou ou nao 
                {
                    /// só receber valores que existam no vetor ou truncar para o próximo que exista 
                    for (int i = posmenor; i < posmaior; i++)
                    {
                        if (kalpha_interp_cont.ElementAt(i) > 0.0)
                            sumkalpha += (kalpha_interp_cont.ElementAt(i) * (kalpha_interp_energia.ElementAt(i+1) - kalpha_interp_energia.ElementAt(i)));

                    }

                }



                posmenor = pos_en_kbeta_min;
                posmaior = pos_en_kbeta_max;


                /// kbeta 
                /// só receber valores que existam no vetor ou truncar para o próximo que exista 
                if (posmenor != -1 && posmaior != -1)
                {
                    for (int i = posmenor; i < posmaior; i++)
                    {
                        if (kbeta_interp_cont.ElementAt(i) > 0.0)
                            sumkbeta += (kbeta_interp_cont.ElementAt(i) * (kbeta_interp_energia.ElementAt(i + 1) - kbeta_interp_energia.ElementAt(i)));
                    }

                    AreaKalpha = sumkalpha;
                    AreaKbeta = sumkbeta;
                    textBox40.Text = AreaKalpha.ToString();
                    textBox41.Text = AreaKbeta.ToString();

                    double ampkalpha = (StrtoDbl(textBox19.Text) - StrtoDbl(textBox18.Text)); /// largura é a diferença entre as energias
                    double ampkbeta = (StrtoDbl(textBox21.Text) - StrtoDbl(textBox20.Text)); /// largura é a diferença entre as energias

                    textBox35.Text =  (sumkalpha/(sumkbeta)).ToString();

                    double erroka, errokb;


                    erroka = DesvioPadrao(kalpha_contagem);
                    errokb = DesvioPadrao(kbeta_contagem);
                    textBox11.Text = (erroka / errokb).ToString();
                    textBox12.Text = (erroka / errokb).ToString();



                    //////////////////////////////////
                    /// metodo 3 Numerical Methros 1 ed 2010 7.33 pag 251 
                    /// Area calculada pela metodologia weedle :  ERRADO , CORRIGIR ! 
                    /// base * altura ( onde base é a subtracao entre as energias a esquerda e direita e a altura é o numero de contagens do pico 
                    /// 
                    /////////////////////////////////
                    AreaKalpha = Weedle_corrigido(kalpha_interp_energia, kalpha_interp_cont, pos_en_kalpha_min, pos_en_kalpha_max);
                    AreaKbeta = Weedle_corrigido(kbeta_interp_energia, kbeta_interp_cont, pos_en_kbeta_min, pos_en_kbeta_max);

                    textBox37.Text = (AreaKalpha).ToString();
                    textBox38.Text = (AreaKbeta).ToString();

                    textBox36.Text = (AreaKalpha / AreaKbeta).ToString();






                    /* DEPRECATED 

                    //////////////////////////////////
                    /// metodo 2
                    /// Area calculada pela metodologia do zani : 
                    /// base * altura ( onde base é a subtracao entre as energias a esquerda e direita e a altura é o numero de contagens do pico 
                    /// 
                    /////////////////////////////////
                    int pospico = Convert.ToInt32(numericUpDown1.Value - 1);
                    double pico = picos.ElementAt(pospico).Key;  /// pega o pico selecionado pelo numericupdown
                    double alturakalpha = pico;  /// chama o pico de altura 
                    double largurakalpha = (StrtoDbl(textBox19.Text) - StrtoDbl(textBox18.Text)); /// largura é a diferença entre as energias

                    pospico = Convert.ToInt32(numericUpDown2.Value - 1);
                    pico = picos.ElementAt(pospico).Key;  /// pega o pico selecionado pelo numericupdown
                    double alturakbeta = pico;           /// chama o pico de altura 
                    double largurakbeta = (StrtoDbl(textBox21.Text) - StrtoDbl(textBox20.Text)); /// largura é a diferença entre as energias

                    textBox37.Text = (alturakalpha * largurakalpha).ToString();
                    textBox38.Text = (alturakbeta * largurakbeta).ToString();

                    textBox36.Text = ((alturakalpha * largurakalpha) / (alturakbeta * largurakbeta)).ToString();
                    


                    //////////////////////////////////
                    /// metodo 3
                    /// Area calculada pela metodologia do zani : 
                    /// base * altura ( onde base é a subtracao entre as energias a esquerda e direita e a altura é o numero de contagens do pico 
                    /// 
                    /////////////////////////////////
                    int pospico = Convert.ToInt32(numericUpDown1.Value - 1);
                    double pico = picos.ElementAt(pospico).Key;  /// pega o pico selecionado pelo numericupdown
                    double alturakalpha = pico;  /// chama o pico de altura 
                    double largurakalpha = (StrtoDbl(textBox19.Text) - StrtoDbl(textBox18.Text)); /// largura é a diferença entre as energias

                    pospico = Convert.ToInt32(numericUpDown2.Value - 1);
                    pico = picos.ElementAt(pospico).Key;  /// pega o pico selecionado pelo numericupdown
                    double alturakbeta = pico;           /// chama o pico de altura 
                    double largurakbeta = (StrtoDbl(textBox21.Text) - StrtoDbl(textBox20.Text)); /// largura é a diferença entre as energias

                    textBox37.Text = (alturakalpha * largurakalpha).ToString();
                    textBox38.Text = (alturakbeta * largurakbeta).ToString();

                    textBox36.Text = ((alturakalpha * largurakalpha) / (alturakbeta * largurakbeta)).ToString();

                     */ //DEPRECATED 











                }




            }

        }


        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private int DecideValor(double valor, double nada )
        {

            return 0; 

        }
        /// <summary>
        ///  Corrige os vetores contagem e a energia na posição da fronteira da LMH 
        ///  para os valores médios entre pos+1 e pos  ou pos-1 e pos 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="vet"></param>
        /// <param name="referencia"></param>
        private void CorrecaoInterpLinearKAKB(int pos, out int poscorr,  List<double> en,List<double> cont, int pico ,int referencia  )
        {

             
            int range = 2 ; 
            /// comparar com o valor a meia altura 
            /// 
            ////////////////////////////////////////////////////
            // energiaxcontagemxposen = vetor onde eu guardo os picos 
            // energiaxcontagemxposen.Key = o par "energia,contagem"
            // energiaxcontagemxposen.Key.Key = energia 
            // energiaxcontagemxposen.Key.Value = Contagem 
            // energiaxcontagemxposen.Value = Posicao do pico no vetor energia ou contagem ( mesmo indice)   
            //////////////////////////////////////////////////////

            double LMH = (picos.ElementAt(pico-1).Key)/2.0;
            double ent1 = 0.0, ent2 = 0.0, cont1 = 0.0, cont2 = 0.0; 

            /// verifica qual das contagens está mais próxima da LMH 
            /// 
            double temp , elemento; 
            double menor = Math.Abs(cont.ElementAt(pos-range) - LMH);
            poscorr = (pos - range); 
            for (int i = -range+1; i <= range; i++) /// nao comparo com o primeiro 
            {
                elemento = cont.ElementAt(pos + i);
                temp = (Math.Abs(elemento - LMH));
                if (menor > temp) 
                {
                    menor = temp;
                    poscorr = pos + i;
                }
                
            }

            ent1 = en.ElementAt(poscorr );
            cont1 = cont.ElementAt(poscorr);
            if (cont1 <= LMH)
            {
                ent2 = en.ElementAt(poscorr + referencia);
                cont2 = cont.ElementAt(poscorr + referencia);
            }
            else
            {
                ent2 = en.ElementAt(poscorr - referencia);
                cont2 = cont.ElementAt(poscorr - referencia);
            }

            
            
            
            
            
            


          //  MessageBox.Show( ent1.ToString() + "  " + ent2.ToString() + "  " + cont1.ToString() + "  " + cont2.ToString() ); 
           en[poscorr] = (ent1 + ent2) / 2.0;
           cont[poscorr] = (cont1 + cont2) / 2.0; 

            


         

        }
        /// <summary>
        /// Calculates standard deviation, same as MATLAB std(X,0) function
        /// <seealso cref="http://www.mathworks.co.uk/help/techdoc/ref/std.html"/>
        /// </summary>
        /// <param name="values">enumumerable data</param>
        /// <returns>Standard deviation</returns>
        public static double DesvioPadrao(List<double> valueList)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 0;
            foreach (double value in valueList)
            {
                k++;
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
            }
            return Math.Sqrt(S / (k - 1));
        }

        /// <summary>
        /// DPMCA help , Analyzing DAta -> Peak Calculations -> Variance FWHM Analysis
        /// </summary>
        /// <param name="energia"></param>
        /// <param name="Contagens"></param>
        /// <returns></returns>
      


        /// <summary>
        /// Busca dentro dos vetores kalpha e kbeta 
        /// A posição onde encontramos a energia correspondente a LMH 
        /// Preenche os textbox com esta energia e sua respectiva contagem 
        /// 
        /// OBS : encontra menor que retorna sempre 
        /// 
        /// </summary>
        private void AtualizaContagem()
        {
            int posicao , poscorr;

            button5.Enabled = false; 

            if (checkBox2.Checked == true) /// usa dados interpolados 
            {
                          
                        /// busca a posicao dentro do vetor interpolado em posicao que encontra-se esta energia 
                        posicao = Encontra_Menor_que(kalpha_interp_energia, StrtoDbl(textBox18.Text));
                        
                        //MessageBox.Show("pos encontrada " + posicao.ToString() + " " + StrtoDbl(textBox18.Text).ToString());
                        if ((posicao <= kalpha_interp_cont.Count) && (posicao >= 0))
                        {
                            CorrecaoInterpLinearKAKB(posicao,out poscorr, kalpha_interp_energia, kalpha_interp_cont , Convert.ToInt32(numericUpDown1.Value) , 1  );  
                             
                            textBox18.Text = kalpha_interp_energia.ElementAt(posicao).ToString();
                            textBox29.Text = kalpha_interp_cont.ElementAt(posicao).ToString();
                          

                            pos_en_kalpha_min = posicao;


                        }
                        else
                        {
                            textBox29.Text = "-1";  MessageBox.Show(" Problemas com en_min kalpha interp");
                        }

                        posicao = Encontra_Maior_que(kalpha_interp_energia, StrtoDbl(textBox19.Text));
                        
                        if ((posicao <= kalpha_interp_cont.Count) && (posicao >= 0))
                        {

                            CorrecaoInterpLinearKAKB(posicao, out poscorr, kalpha_interp_energia, kalpha_interp_cont, Convert.ToInt32(numericUpDown1.Value),-1); //// se ainda nao corrigiu os quatro  vetores 

                            textBox19.Text = kalpha_interp_energia.ElementAt(posicao).ToString();
                            textBox30.Text = kalpha_interp_cont.ElementAt(posicao).ToString();
                            pos_en_kalpha_max = posicao;
                        }

                        else
                        {
                            textBox30.Text = "-1";
                            MessageBox.Show(" Problemas com en_max kalpha interp");
                        }

                        posicao = Encontra_Menor_que(kbeta_interp_energia, Convert.ToDouble(textBox20.Text));
                        
                        if ((posicao <= kbeta_interp_cont.Count) && (posicao >= 0))
                        {
                            CorrecaoInterpLinearKAKB(posicao, out poscorr, kbeta_interp_energia, kbeta_interp_cont, Convert.ToInt32(numericUpDown2.Value),1); //// se ainda nao corrigiu os quatro  vetores 

                            textBox20.Text = kbeta_interp_energia.ElementAt(posicao).ToString();
                            textBox31.Text = kbeta_interp_cont.ElementAt(posicao).ToString();
                            pos_en_kbeta_min = posicao;
                        }
                        else
                        {
                            textBox31.Text = "-1";
                            MessageBox.Show(" Problemas com en_min kbeta interp");
                        }

                        posicao = Encontra_Maior_que(kbeta_interp_energia, Convert.ToDouble(textBox21.Text));
                        
                        if ((posicao <= kbeta_interp_cont.Count) && (posicao >= 0))
                        {
                            CorrecaoInterpLinearKAKB(posicao, out poscorr, kbeta_interp_energia, kbeta_interp_cont, Convert.ToInt32(numericUpDown2.Value),-1); //// se ainda nao corrigiu os quatro  vetores 
                           
                            textBox21.Text = kbeta_interp_energia.ElementAt(posicao).ToString();
                            textBox32.Text = kbeta_interp_cont.ElementAt(posicao).ToString();
                            pos_en_kbeta_max = posicao;
                        }
                        else
                        {
                            textBox32.Text = "-1";
                            MessageBox.Show(" Problemas com en_max kbeta interp");
                        }



               


            }


            else/// usa dados nao interpolados 
            {
                        /// busca a posicao dentro do vetor nao interpolado em posicao que encontra-se esta energia 
                        /// 


                        posicao = Encontra_Menor_que(energia, StrtoDbl(textBox18.Text));
                        textBox29.Text = contagem.ElementAt(posicao).ToString();

                        if ((posicao <= energia.Count) && (posicao >= 0))
                        {
                            textBox29.Text = contagem.ElementAt(posicao+1).ToString();
                            pos_en_kalpha_min = posicao;
                        }
                        else
                        {
                            textBox29.Text = "-1";
                            MessageBox.Show(" Problemas com en_min kalpha interp");
                        }


                        posicao = Encontra_Maior_que(energia, StrtoDbl(textBox19.Text));
                        textBox30.Text = contagem.ElementAt(posicao).ToString();

                        if ((posicao <= energia.Count) && (posicao >= 0))
                        {
                            textBox30.Text = contagem.ElementAt(posicao).ToString();
                            pos_en_kalpha_max = posicao - 1;
                        }
                        else
                        {
                            textBox30.Text = "-1";
                            MessageBox.Show(" Problemas com en_min kalpha interp");
                        }

                        posicao = Encontra_Menor_que(energia, StrtoDbl(textBox20.Text));
                        textBox31.Text = contagem.ElementAt(posicao).ToString();

                        if ((posicao <= energia.Count) && (posicao >= 0))
                        {
                            textBox31.Text = contagem.ElementAt(posicao+1).ToString();
                            pos_en_kbeta_min = posicao;
                        }
                        else
                        {
                            textBox31.Text = "-1";
                            MessageBox.Show(" Problemas com en_min kalpha interp");
                        }


                        posicao = Encontra_Maior_que(energia, StrtoDbl(textBox21.Text));
                        textBox32.Text = contagem.ElementAt(posicao).ToString();

                        if ((posicao <= energia.Count) && (posicao >= 0))
                        {
                            textBox32.Text = contagem.ElementAt(posicao).ToString();
                            pos_en_kbeta_max = posicao;
                        }
                        else
                        {
                            textBox32.Text = "-1";
                            MessageBox.Show(" Problemas com en_min kalpha interp");
                        }

            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (button5.Enabled == false || textBox29.Text == "-1" || textBox30.Text == "-1" || textBox31.Text == "-1" ||  textBox32.Text == "-1")
            {
                MessageBox.Show("Refaça grafico ou gere novos picos"); 
            }
            else
            {
                AtualizaContagem();
                calcularazao = 1;
                RecalculaRazao();
                calcularazao = 0;

            }
            

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

        private void label38_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            double esq, dir;
            if (picos.Count() >= (numericUpDown1.Value) && numericUpDown1.Value > 0)
            {
                esq = picos.ElementAt(Convert.ToInt32(numericUpDown1.Value - 1)).Value.Key;
                dir = picos.ElementAt(Convert.ToInt32(numericUpDown1.Value - 1)).Value.Value;

                textBox18.Text = esq.ToString(); // valor a esquerda 
                textBox19.Text = dir.ToString(); // valor a direita 

            }




        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            double esqa, dira;
            if (picos.Count() >= (numericUpDown2.Value) && numericUpDown2.Value > 0)
            {
                esqa = picos.ElementAt(Convert.ToInt32(numericUpDown2.Value - 1)).Value.Key;
                dira = picos.ElementAt(Convert.ToInt32(numericUpDown2.Value - 1)).Value.Value;

                textBox20.Text = esqa.ToString(); // valor a esquerda 
                textBox21.Text = dira.ToString(); // valor a direita 

            }
        }

        private void label19_Click_1(object sender, EventArgs e)
        {

        }

        private void label58_Click(object sender, EventArgs e)
        {

        }

     

     

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {

        }

        private void label67_Click(object sender, EventArgs e)
        {

        }

        private void label57_Click(object sender, EventArgs e)
        {

        }


        void SalvaMaterial (List<string> ent)
        {
            int numlinhas =  dataGridView1.RowCount-1; 

            for (int i = 0 ; i < numlinhas ; i ++ )
            {
                ent.Add(dataGridView1.Rows[i].Cells[0].Value.ToString());
                ent.Add(dataGridView1.Rows[i].Cells[1].Value.ToString());
                ent.Add(dataGridView1.Rows[i].Cells[2].Value.ToString());
                ent.Add(dataGridView1.Rows[i].Cells[3].Value.ToString());
                ent.Add(dataGridView1.Rows[i].Cells[4].Value.ToString());
                ent.Add(dataGridView1.Rows[i].Cells[5].Value.ToString());
            }

        }
        void RecolheDados(List<string> ent)
        {
            ent.Clear();
            if (opcao_metodologia == 1)
            {
                ent.Add(textBoxM1IB.Text); /// I_B
                ent.Add(textBoxM1IR.Text); /// IR
                ent.Add(textBoxM1rhorev.Text); /// rho rev 
                ent.Add(textBoxM1Kalpha.Text); /// Energia kalpha da base 
                ent.Add(textBoxM1E0.Text); /// E0
                ent.Add(textBoxM1muent.Text); /// theta 
            }

            if (opcao_metodologia == 2)
            {

                ent.Add(textBoxM2IB.Text); /// I_B
                ent.Add(textBoxM2IR.Text); /// IR
                ent.Add(textBoxM2rhorev.Text); /// rho rev 
                
                ent.Add(textBoxM2kalpha.Text); /// en kalpha
                ent.Add(textBoxM2kbeta.Text); /// en kbeta
            }
            if (opcao_metodologia == 3)
            {

                /// salvar os dados 
                /// 
                ent.Add(textBoxM3IB.Text); /// I_B
                ent.Add(textBoxM3IR.Text); /// IR
                ent.Add(textBoxM3rhorev.Text); /// rho rev 
                ent.Add(textBoxM3rhobase.Text); /// rho rev 
                ent.Add(textBoxM3E0.Text); /// 
                ent.Add(textBoxM3Kalpha.Text);                           /// 
                ent.Add(textBoxmubasekalpha.Text);                           /// 
                ent.Add(textBoxPEBaseE0.Text);                           /// 
                ent.Add(textBoxmubaseE0.Text);                           /// 
                ent.Add(textBoxmurevE0.Text);                           /// 
                ent.Add(textBoxmurevkalphabase.Text);                           /// 
                ent.Add(textBoxM3mu1.Text);                           /// 
                ent.Add(textBoxM3mu2.Text);                           /// 

                //// salvar os elementos 
                SalvaMaterial(ent);  

            
            }

        }
       

        private double Espessura_Met1()
        {
            double espessura = 0.0;

            double IB = Convert.ToDouble(textBoxM1IB.Text),// kalpha / k beta com revestimento 
                   IR = Convert.ToDouble(textBoxM1IR.Text), // kalpha / k beta sem revestimento 
                   rhorev = Convert.ToDouble(textBoxM1rhorev.Text), // densidade do  revestimento 
                   mu_energiaka = Convert.ToDouble(textBoxmurevkalphabase.Text),// kalpha / k beta sem revestimento 
                   mu_energiaE0 = Convert.ToDouble(textBoxmurevE0.Text),
                   ang = Convert.ToDouble(textBoxM1muent.Text);



            espessura = (Math.Log(IR / IB)) * (Math.Cos(ang) / (rhorev * (mu_energiaE0 + mu_energiaka)));

            return espessura;
        }


        private double Espessura_Met2()
        {
            double espessura = 0.0;

            double IB = Convert.ToDouble(textBoxM2IB.Text),
                   IR = Convert.ToDouble(textBoxM2IR.Text),
                   rhorev = Convert.ToDouble(textBoxM2rhorev.Text),
                   mu_energiaka = Convert.ToDouble(textBoxmurevkalphabase.Text), /// kalpha 
                   mu_energiakb = Convert.ToDouble(textBoxmurevkbetabase.Text);  /// k beta


            espessura = (Math.Log(IR / IB) * (1.000) / (rhorev * (mu_energiakb - mu_energiaka)));

            return espessura;
        }

        private double Espessura_Met3()
        {
            double espessura = 0.0;

            double IB = Convert.ToDouble(textBoxM3IB.Text),
                   IR = Convert.ToDouble(textBoxM3IR.Text),
                   rhorev = Convert.ToDouble(textBoxM3rhorev.Text),
                   rhobase = Convert.ToDouble(textBoxM3rhobase.Text),

                   mu_KA_base = rhobase * Convert.ToDouble(textBoxmubasekalpha.Text), /// mu_kalpha
                   tau_EO_base = Convert.ToDouble(textBoxPEBaseE0.Text), /// taui  
                   mu_E0_base = rhobase * Convert.ToDouble(textBoxmubaseE0.Text),  /// mu0
                   mu_E0_rev = rhorev * Convert.ToDouble(textBoxmurevE0.Text),  /// mu0'
                   mu_KA_rev = rhorev *  Convert.ToDouble(textBoxmurevkalphabase.Text),   //// mu_kalpha'
                   
                   A,    /// temporarios  
                   B ,   /// temporarios 
                   theta1 = Convert.ToDouble(textBoxM3mu1.Text) , 
                   theta2 = Convert.ToDouble(textBoxM3mu2.Text);



            A = (tau_EO_base / Math.Sin(theta1)) / (mu_E0_base / Math.Sin(theta1) + mu_KA_base / Math.Sin(theta2));
            B = (mu_E0_rev / Math.Sin(theta1)) + (mu_KA_rev / Math.Sin(theta1)); 

            espessura = -1.0000 * (Math.Log(IR / (IB*A)) / (B));

            return espessura;
        }
        private void button1_Click(object sender, EventArgs e)
        {

            
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label74_Click(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void CarregaMaterial(System.IO.StreamReader file)
        {
           
            
            

            ///limpa 
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            dataGridView1.ColumnCount = 6;

            /// carrega
            
          
            /// colunas 
            dataGridView1.Columns[0].Name = "Número atômico";
            dataGridView1.Columns[1].Name = "Nome do elemento";
            dataGridView1.Columns[2].Name = "Densidade";
            dataGridView1.Columns[3].Name = "Peso Atômico";
            dataGridView1.Columns[4].Name = "Peso Relativo ";
            dataGridView1.Columns[5].Name = "Base ? ";

            string num_atom = "", nome = "", dens = "", pes_mol = "", pes_rela = "", subs = ""; 
            
            using (file)
            {
                int cont = 0; 
                string line;
                while ((line = file.ReadLine()) != null)
                {

                    if (cont == 0) num_atom = line;
                    if (cont == 1) nome = line;
                    if (cont == 2) dens = line;
                    if (cont == 3) pes_mol = line;
                    if (cont == 4) pes_rela = line;
                    if (cont == 5) subs = line; 

                    cont++;
                     if (cont == 6) /// adiciona a linha e reinicia para proximo elemento 
                     {
                         string[] row = new string[] { num_atom, nome, dens, pes_mol, pes_rela, subs };
                         dataGridView1.Rows.Add(row);
                         cont = 0; 
                     } 
                }
            }


            

            /// Linhas 
           

        }

        private void Preenche_Dados_entrada(System.IO.StreamReader file)
        {
            string line;


            /// preenche dados do material , independe do modelo 
            /// 



            if (opcao_metodologia == 1 )
            {
                line = file.ReadLine();
                textBoxM1IB.Text = line;
                line = file.ReadLine();
                textBoxM1IR.Text = line;
                line = file.ReadLine();
                textBoxM1rhorev.Text = line;
                line = file.ReadLine();
                textBoxM1Kalpha.Text = line;
                line = file.ReadLine();
                textBoxM1E0.Text = line;
            }

            if (opcao_metodologia == 2)
            {

                line = file.ReadLine();
                textBoxM2IB.Text = line;
                line = file.ReadLine();
                textBoxM2IR.Text = line;
                line = file.ReadLine();
                textBoxM2rhorev.Text = line;
                line = file.ReadLine();
                textBoxM2kalpha.Text = line;
                line = file.ReadLine();
                textBoxM2kbeta.Text = line;


            }

            if (opcao_metodologia == 3)
            {


                /// carrega dados 

                line = file.ReadLine();
                textBoxM3IB.Text = line; 
                line = file.ReadLine();
                textBoxM3IR.Text = line; 
                line = file.ReadLine();
                textBoxM3rhorev.Text = line;
                line = file.ReadLine();
                textBoxM3rhobase.Text = line;
                line = file.ReadLine();
                textBoxM3E0.Text = line;  
                line = file.ReadLine();
                textBoxM3Kalpha.Text = line; 
                line = file.ReadLine();
                textBoxmubasekalpha.Text = line;                            
                line = file.ReadLine();
                textBoxPEBaseE0.Text = line;  
                line = file.ReadLine();
                textBoxmubaseE0.Text = line;                                
                line = file.ReadLine();
                textBoxmurevE0.Text = line;  
                line = file.ReadLine();
                textBoxmurevkalphabase.Text = line;
                line = file.ReadLine();
                textBoxM3mu1.Text = line;
                line = file.ReadLine();
                textBoxM3mu2.Text = line; 
                                            /// 


                /// carrega materiais 
                CarregaMaterial(file);


            }

        }
        private void button4_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            string path = Directory.GetCurrentDirectory();
            openFileDialog1.InitialDirectory = path;
            openFileDialog1.RestoreDirectory = true;

            

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {


                Stream myStream = openFileDialog1.OpenFile();
                string nome = openFileDialog1.FileName;
                
                int contador = 0, carregado = 0;
                try
                {
                    if (myStream != null)
                    {
                        using (myStream)
                        {
                            nome_arquivo = nome;
                            System.IO.StreamReader file = new System.IO.StreamReader(nome_arquivo);
                            Preenche_Dados_entrada(file);
                            file.Close();
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                }
            }
        }

        private void textBox44_TextChanged(object sender, EventArgs e)
        {
            textBox2.Text = (Convert.ToDouble(textBox44.Text) * 10000.0).ToString(); /// transforma em micrometros
        }

        private void textBox22_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox16_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox33_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox17_TextChanged(object sender, EventArgs e)
        {

        }


        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            /*
            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;
            tooltip.RemoveAll();
            prevPosition = pos;
            var results = chart1.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        //if (Math.Abs(pos.X - pointXPixel) < 2 &&
                        //    Math.Abs(pos.Y - pointYPixel) < 2)
                        {
                            tooltip.Show("X=" + prop.XValue + ", Y=" + prop.YValues[0], this.chart1,
                                            pos.X, pos.Y - 15);
                        }
                    }
                }
            }
             * */
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {



        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Activated(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "C:\\Users\\LR\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\Espessura\\bin\\Release\\";
            openFileDialog1.Filter = "config files (*.conf)|*.conf";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;



            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    /// Tenta ler o arquivo 
                    /// 
                    CarregaValores(openFileDialog1.OpenFile(), openFileDialog1.FileName);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                }
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog2 = new SaveFileDialog();
            saveFileDialog2.Filter = "config files (*.conf)|*.conf";

            string nome = "C:\\Users\\LR\\Google Drive\\LuizRosalba pc na uerj\\Espessura\\Espessura\\bin\\Release\\origval.out";                      /// 
            string path = Directory.GetCurrentDirectory();

            saveFileDialog2.InitialDirectory = path;
            saveFileDialog2.ShowDialog();

            string name = saveFileDialog2.FileName;
            if (name != "")
            {
                GravaValores(name);
            }

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void mCNPXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            opcao_programa = 2; /// Geant4 selecionado 

            Stream myStream = null;
            List<double> canal = new List<double>(); /// vetor que guardara as contagens em cada canal

            /// Abrindo o arquivo 
            
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = dir_inicial;
           
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
           // openFileDialog1.RestoreDirectory = true;
            
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)/// se abriu com sucesso 
            {

                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        LimpaTudo();
                        LimpaOriginal();
                        using (myStream)
                        {

                            /// MessageBox.Show(openFileDialog1.FileName); 

                            nome_arquivo = openFileDialog1.FileName;

                            string text = System.IO.File.ReadAllText(openFileDialog1.FileName);
                            //Arquivo_aberto.AppendText(nome_arquivo);
                            //Arquivo_aberto.AppendText(System.Environment.NewLine);
                            //Arquivo_aberto.AppendText(text);

                            string[] lines = System.IO.File.ReadAllLines(openFileDialog1.FileName);
                            int i = 0;

                            energia.Clear();
                            contagem.Clear();

                            ///abrindo o arquivo em duas colunas e convertendo para double 
                            foreach (string linha in lines)
                            {
                                string[] separado = linha.Split(new Char[] { ' ', '\t' }); /// separa a linha  tabulacao 

                                foreach (string dado in separado)   /// para cada dado na linha 
                                {

                                    if (dado.Trim() != "")  // se nao esta nulo 
                                    {
                                        if (i == 0) /// se é o primeiro dado adiciona para o vetor energia 
                                        {
                                            energia.Add(StrtoDbl(dado));  /// adiciona para o vetor energia nao interpolado 
                                            energia_original.Add(StrtoDbl(dado));
                                            i = 1;
                                        }
                                        else  /// senao adiciona para o vetor contagem nao interpolado 
                                        {
                                            contagem.Add(StrtoDbl(dado));
                                            contagem_original.Add(StrtoDbl(dado));
                                            i = 0;
                                        }
                                    }

                                }



                            }


                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: Nao e possivel ler a partir do arquivo. Original error: " + ex.Message);
                }
            }
        }

        private void label43_Click(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            //RecarregaArquivo();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            /// 
            using (var form = new Material())
            {
                //Material material = new Material();
                var result = form.ShowDialog();

                if (result == DialogResult.Cancel)
                {
                    
                    densidade = form.RetornaDensidade;            //values preserved after close
                    nome_elemento = form.RetornaNome;
                    Z = form.RetornaZ;
                    peso_molecular = form.RetornaPesoMolecular;
                    bool substrato = form.RetornaBaseouRevestimento;
                    peso_relativo = form.RetornaPesoRelativo;

                    if (Z!=0)
                    {
                        dataGridView1.ColumnCount = 6;

                        /// colunas 
                        dataGridView1.Columns[0].Name = "Número atômico";
                        dataGridView1.Columns[1].Name = "Nome do elemento";
                        dataGridView1.Columns[2].Name = "Densidade";
                        dataGridView1.Columns[3].Name = "Peso Atômico";
                        dataGridView1.Columns[4].Name = "Peso Relativo ";
                        dataGridView1.Columns[5].Name = "Base ? ";

                        /// Linhas 
                        string[] row = new string[] { Z.ToString(), nome_elemento, densidade.ToString(),peso_molecular.ToString() , peso_relativo.ToString() ,substrato.ToString() };
                        dataGridView1.Rows.Add(row);



                        //listView1.Items.Add(nome_elemento).SubItems.AddRange(row1);
                        //listView1.Items.Add(densidade.ToString()).SubItems.AddRange(row1);

                        /*
                        ListViewItem item1 = new ListViewItem("Something");
                        item1.SubItems.Add("SubItem1a");
                        item1.SubItems.Add("SubItem1b");
                        item1.SubItems.Add("SubItem1c");
                        */

                        //string dateString = form.ReturnValue2;
                        //Do something here with these values

                        //for example
                        //this.txtSomething.Text = val;

                    }
                   
                }
            }
            
           

            /// Seleciona elemento 
            /// 
            /// Mostra densidade 
            /// 
            /// Insere peso relativo 
            /// 

        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            ///Given an element Z and an energy E, returns the mass-energy absorption cross section in cm2g
            /*int Z = Convert.ToInt16(textBox1.Text);
            if (textBox1.Text != "")
            {
                if (Z <= 111 && Z >= 1)
                {
                    if (textBox3.Text > 0 ) 
                    {
                        textBox5.Text = xl.CS_Total(Z,);
                    }
                        
                }
            
            }
             */
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
           

        }

        private void checkBox8_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBox1.Enabled == false)
                checkBox1.Enabled = true;
            else
                checkBox1.Enabled = false;
             
        }

        private void textBoxE_TextChanged(object sender, EventArgs e)
        {

        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void textBoxC_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxF_TextChanged(object sender, EventArgs e)
        {

        }

        private void label70_Click(object sender, EventArgs e)
        {

        }

        private void label27_Click_1(object sender, EventArgs e)
        {

        }

        private void label77_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog2 = new SaveFileDialog();
            List<string> dados_espessura = new List<string>();
            string path = Directory.GetCurrentDirectory();
            saveFileDialog2.InitialDirectory = path;
            saveFileDialog2.DefaultExt = "sps";
            saveFileDialog2.Filter = "Espessura (*.sps)|*.sps";
            saveFileDialog2.ShowDialog();
            string name = saveFileDialog2.FileName;
            if (name != "")
            {
                RecolheDados(dados_espessura);
                Escreve_dados_espessura(dados_espessura, name);
            }
        }

        private void textBox47_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            double espessura = 0.0;

                               


            if (opcao_metodologia == 1)
            {
                 
                energia_E0 = Convert.ToDouble(textBoxM1E0.Text);
                energiaKalphabase = Convert.ToDouble(textBoxM1Kalpha.Text);

                int Zrev = 30;  // zinco  VIrá da selecao do material na tabela 
                /// acertar para pegar o z do revestimento 
                textBoxmurevE0.Text = xl.CS_Total(Zrev, energia_E0).ToString();
                textBoxmurevkalphabase.Text = xl.CS_Total(Zrev, energiaKalphabase).ToString();
                textBoxmurevkbetabase.Text = " " ;

                espessura = Espessura_Met1();
                textBox44.Text = espessura.ToString();

            }
            if (opcao_metodologia == 2)
            {
                
                energiaKalphabase = Convert.ToDouble(textBoxM2kalpha.Text);
                energiaKbetabase = Convert.ToDouble(textBoxM2kbeta.Text);
                
                int Zrev = 30;  // zinco 

                /// acertar para pegar o z do revestimento 
                textBoxmurevE0.Text = " ";
                textBoxmurevkalphabase.Text = xl.CS_Total(Zrev, energiaKalphabase).ToString();
                textBoxmurevkbetabase.Text = xl.CS_Total(Zrev, energiaKbetabase).ToString();


                espessura = Espessura_Met2();
                textBox44.Text = espessura.ToString();
            }

            if (opcao_metodologia == 3)
            {



                espessura = Espessura_Met3();
                textBox44.Text = espessura.ToString();
            }


        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
           
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void textBoxM3mu2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label96_Click(object sender, EventArgs e)
        {

        }

        private void textBoxM3mu1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label95_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            int selectedcolumnindex = dataGridView1.SelectedCells[0].ColumnIndex;


            if (dataGridView1.SelectedRows.Count > 0 && (selectedrowindex < dataGridView1.Rows.Count-1))
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
            }


            using (var form = new Material())
            {

                
                //Material material = new Material();
                var result = form.ShowDialog();

                if (result == DialogResult.Cancel)
                {

                    densidade = form.RetornaDensidade;            //values preserved after close
                    nome_elemento = form.RetornaNome;
                    Z = form.RetornaZ;
                    peso_molecular = form.RetornaPesoMolecular;
                    bool substrato = form.RetornaBaseouRevestimento;

                    if (Z != 0)
                    {
                        
                        dataGridView1.ColumnCount = 6;

                        /// colunas 
                        dataGridView1.Columns[0].Name = "Número atômico";
                        dataGridView1.Columns[1].Name = "Nome do elemento";
                        dataGridView1.Columns[2].Name = "Densidade";
                        dataGridView1.Columns[3].Name = "Peso Atômico";
                        dataGridView1.Columns[4].Name = "Peso Relativo";
                        dataGridView1.Columns[5].Name = "Base ? ";

                        /// Linhas 
                        string[] row = new string[] { Z.ToString(), nome_elemento, densidade.ToString(), peso_molecular.ToString() ,  peso_relativo.ToString(), substrato.ToString() };
                        dataGridView1.Rows.Add(row);



                    }

                }

            }
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
           
            


        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
           
            if (dataGridView1.CurrentRow.Index < dataGridView1.Rows.Count)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                int selectedcolumnindex = dataGridView1.SelectedCells[0].ColumnIndex;

                if (selectedcolumnindex >0 ) // evita pegar o cabeçalho 
                   {
                       
                        
              
                       DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];

                
                        
                       //string elemento  = Convert.ToString(selectedRow.Cells[0].Value);
                       //string nome  = Convert.ToString(selectedRow.Cells[1].Value);
                       int numatomico = Convert.ToInt16(selectedRow.Cells[0].Value);
                       double densidade = Convert.ToDouble(selectedRow.Cells[2].Value);   
                       bool ehbase = Convert.ToBoolean(selectedRow.Cells[5].Value);

                        
                        if (ehbase)
                        {
                            textBoxM3rhobase.Text = densidade.ToString(); //// rho base 
                            textBoxPEBaseE0.Text = textBoxPE.Text;  /// Photo_E0
                            textBoxmubaseE0.Text = textBoxTT.Text;  /// mu_0 
                            textBoxmubasekalpha.Text = textBoxTTkalpha.Text; /// mu_kalpha                                   
                        }
                        else
                        {
                            textBoxM3rhorev.Text = densidade.ToString();
                            textBoxmurevE0.Text = textBoxTT.Text;  /// mu_0'
                            textBoxmurevkalphabase.Text = textBoxTTkalpha.Text; /// mu_kalpha'                                     
                                                                   
                        }
                       
                        

                       if (textBox_E0_SC.Text != "" && textBox_E0_SC.Text != null)
                       {
                           /// depois fazer para composto 
                           double ene = Convert.ToDouble(textBox_E0_SC.Text);
                           textBoxPE.Text = xl.CS_Photo(numatomico, ene).ToString();
                           textBoxCP.Text = xl.CS_Compt(numatomico, ene).ToString();
                           textBoxRL.Text = xl.CS_Rayl(numatomico, ene).ToString();
                           textBoxTT.Text = xl.CS_Total(numatomico, ene).ToString();

                       }

                       if (textBox_KALPHA_SC.Text != "" && textBox_KALPHA_SC.Text != null)
                       {
                           /// depois fazer para composto 
                           double ene = Convert.ToDouble(textBox_KALPHA_SC.Text);
                           textBoxPE_KA.Text = xl.CS_Photo(numatomico, ene).ToString();
                           textBoxCPKA.Text = xl.CS_Compt(numatomico, ene).ToString();
                           textBoxRLKA.Text = xl.CS_Rayl(numatomico, ene).ToString();
                           textBoxTTkalpha.Text = xl.CS_Total(numatomico, ene).ToString();
                       }


                       /// https://github.com/tschoonj/xraylib/wiki/Code-examples
                    /// 
                   //Z  = dataGridView1.SelectedCells.; 
                    /// aquiiiii
                    /*
                    if (textBox_E0_SC.Text!=" ")  /// depois verificar por numeros 
                    {
                        ///http://lvserver.ugent.be/xraylib/xraylib-manual.pdf pagina 16 
                        ///Given an element Z and an energy E , these functions will return respectively the
                        ///total absorption cross section, the photoionization cross section, the Rayleigh scattering
                        /// cross section and the Compton scattering cross section, expressed in cm2/g.
                        /// 
                        textBox50.Text = xl.CS_Photo(Z,E0); 
                        textBox46.Text = xl.CS_Compt(Z,E0); 
                        textBox45.Text = xl.CS_Rayl(Z,E0);
                        textBox49.Text = xl.CS_Total(Z, E0); 


                    }                       
                   */


               
                   }

            }
            
           

           
        }

        private void textBoxPE_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBoxM3Kalpha_TextChanged(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void textBoxM3E0_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label91_Click(object sender, EventArgs e)
        {

        }

        private void textBoxkbeta_TextChanged(object sender, EventArgs e)
        {

        }

        private void label102_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label104_Click(object sender, EventArgs e)
        {

        }

        private void Painel_Met_5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox45_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox45_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label110_Click(object sender, EventArgs e)
        {

        }

        private void textBox_E0_SC_TextChanged(object sender, EventArgs e)
        {
            textBoxM3E0.Text = textBox_E0_SC.Text; 
        }

        private void textBox_KALPHA_SC_TextChanged(object sender, EventArgs e)
        {

            textBoxM3Kalpha.Text = textBox_KALPHA_SC.Text; 
        }
    }
}

