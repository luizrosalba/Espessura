using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Science;
  
namespace Espessura
{

   

    public partial class Material : Form
    {
        XrayLib xl = XrayLib.Instance; /// instancia do Xraylib 
        public double RetornaDensidade { get; set; }
        public string RetornaNome { get; set; }                      /// 

        public double RetornaPesoRelativo { get; set; }                      /// 

        public int RetornaZ { get; set; }                      /// 

        public double RetornaPesoMolecular { get; set; }                      /// 

        public bool RetornaBaseouRevestimento { get; set; }                      /// 

        public Material()
        {
            
            InitializeComponent();

            
        }

        int Numatom;
        double peso_molecular;
        double densidade;
        string nome;
        bool substrato ;
        double peso_relativo; 

        private void button1_Click(object sender, EventArgs e)
        {


            if (textBox2.Text != "") peso_relativo = Convert.ToDouble(textBox2.Text); else peso_relativo = 0.0; 
                 
             this.RetornaNome = nome;
             this.RetornaDensidade = densidade;
             this.RetornaZ = Numatom;
             this.RetornaPesoMolecular = peso_molecular;
             this.RetornaPesoRelativo = peso_relativo; 

             if (radioButton1.Checked) substrato = true;
             if (radioButton2.Checked) substrato = false;

            this.RetornaBaseouRevestimento = substrato;

            //double en = Convert.ToDouble(textBox3.Text);
            //double peso_atomico = xl.AtomicWeight(Z); // Given an element Z, returns its atomic weight in g/mol.
            
            //double CSTOTAL = xl.CS_Total(Z,en);//Given an element Z, returns its density at room temperature in g/cm3.


            //textBox2.Text=densidade.ToString();
            //textBox4.Text = CSTOTAL.ToString();
            //double CSPHOTO = xl.ElementDensity(Z);//Given an element Z, returns its density at room temperature in g/cm3.
            //double CSRAY = xl.ElementDensity(Z);//Given an element Z, returns its density at room temperature in g/cm3.
            //double CSCOMP = xl.ElementDensity(Z);//Given an element Z, returns its density at room temperature in g/cm3.

             this.Close();
            
        }   

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                int Z;
                bool success = int.TryParse(textBox1.Text, out Z);

                if (success)
                {
                    if (Z <= 111 && Z >= 1)
                    {

                        ElementData elemento;
                        elemento = xl.GetElementData(Convert.ToInt16(textBox1.Text));

                        textBox3.Text = elemento.Name;
                        textBox8.Text = elemento.Number.ToString();
                        textBox7.Text = elemento.Density.ToString();
                        textBox4.Text = elemento.Weight.ToString();
 
                        Numatom = Z; 
                        nome = elemento.Name; ;
                        densidade = elemento.Density;
                        peso_molecular = elemento.Weight;
                        
                        
                    }
                    else
                    {
                        textBox3.Text = "Elemento inexistente";
                    }

                }
                

            }
           

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
 

        private void Material_Load(object sender, EventArgs e)
        {

            
            ComboboxItem item = new ComboboxItem();
            for (int Z = 1; Z <= 111; Z++   )
            {
                ElementData elemento;
                elemento = xl.GetElementData(Z);
                item.Text = Z.ToString() +  " - " + elemento.Name;
                comboBox1.Items.Add(item);
                //item.Value = 12;
                
            }

            /* if (Z <= 111 && Z >= 1)
             {

                 ElementData elemento;
                 elemento = xl.GetElementData(Convert.ToInt16(textBox1.Text));

                 textBox6.Text = elemento.Name;
                 textBox2.Text = elemento.Density.ToString();
                 textBox5.Text = elemento.Weight.ToString();
             }
             else
             {
                 textBox6.Text = "Elemento inexistente";
             }
         */

        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            ElementData elemento;
            elemento = xl.GetElementData(comboBox1.SelectedIndex+1) ;
            textBox3.Text = elemento.Name;
            textBox8.Text = elemento.Number.ToString(); 
            textBox7.Text = elemento.Density.ToString();
            textBox4.Text = elemento.Weight.ToString();

        }
    }
}
public class ComboboxItem
{
    public string Text { get; set; }
    public object Value { get; set; }

    public override string ToString()
    {
        return Text;
    }
}