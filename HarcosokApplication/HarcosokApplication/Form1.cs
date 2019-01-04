using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HarcosokApplication
{
    public partial class Form1 : Form
    {
        MySqlConnection conn;

        public Form1()
        {
            InitializeComponent();
            conn = new MySqlConnection("Server=localhost;Database=cs_harcosok;Uid=root;Pwd=");
            conn.Open();
            CreateTable();
        }

        public void CreateTable()
        {
            var command = conn.CreateCommand();
            command.CommandText = @"
                                CREATE TABLE IF NOT EXISTS 
                                    harcosok 
                                    (
                                    id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                    nev VARCHAR(100) NOT NULL UNIQUE,
                                    letrehozas DATE NOT NULL
                                    );
                                CREATE TABLE IF NOT EXISTS 
                                    kepessegek 
                                    (
                                    id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                    nev VARCHAR(100) NOT NULL,
                                    leiras VARCHAR(1000) NOT NULL,
                                    harcos_id INTEGER NOT NULL,
                                    FOREIGN KEY (harcos_id) REFERENCES harcosok(id)
                                    );
                                    ";
            command.ExecuteNonQuery();
            AdatFrissit();
        }

        public void LetrehozasButton_Click(object sender, EventArgs e)
        {
            string harcosneve = harcosNeveTextBox.Text.ToString();
            var command = conn.CreateCommand();
            DateTime date = DateTime.Now;
            command.CommandText = @"
                                INSERT INTO `cs_harcosok`.`harcosok` 
                                (
                                `nev`,
                                `letrehozas`
                                ) 
                                VALUES
                                (
                                @nev, 
                                @date
                                );
                                ";
            command.Parameters.AddWithValue("@nev", harcosneve);
            command.Parameters.AddWithValue("@date", date);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySql.Data.MySqlClient.MySqlException)
            {

                MessageBox.Show("Ilyen harcos már létezik");
            }
            AdatFrissit();
        }

        public void AdatFrissit()
        {
            hasznaloComboBox.Items.Clear();
            var command = conn.CreateCommand();
            command.CommandText = @"
                                    SELECT 
                                        nev
                                    FROM 
                                        harcosok
                                    ";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nev = reader.GetString("nev");
                    string sor = string.Format("{0}", nev);
                    hasznaloComboBox.Items.Add(sor);
                }
            }
            AdatFrissitHarcosok();
            AdatFrissitKepessegei();
        }
        public void AdatFrissitHarcosok()
        {
            harcosokListBox.Items.Clear();
            kepessegekListBox.Items.Clear();
            var command = conn.CreateCommand();
            command.CommandText = @"
                                    SELECT 
                                        nev, letrehozas
                                    FROM 
                                        harcosok
                                    ";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nev = reader.GetString("nev");
                    DateTime letrehozas = reader.GetDateTime("letrehozas");
                    string sor = string.Format("{0} {1:yyyy. MM. dd.}", nev, letrehozas);
                    harcosokListBox.Items.Add(sor);
                }
            }
        }
        public void AdatFrissitKepessegei()
        {
            kepessegekListBox.Items.Clear();
            string text = harcosokListBox.GetItemText(harcosokListBox.SelectedItem);
            if (string.IsNullOrEmpty(text)) return;
            text = text.Substring(0, text.Length - 14);
            var command = conn.CreateCommand();
            command.CommandText = @"
                                SELECT kepessegek.nev nev
                                FROM kepessegek 
JOIN harcosok ON harcosok.id = kepessegek.harcos_id
                                WHERE harcosok.nev 
                                LIKE @active
                                ";
            command.Parameters.AddWithValue("@active", text);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nev = reader.GetString("nev");
                    string sor = string.Format("{0}", nev);
                    kepessegekListBox.Items.Add(sor);
                }
            }

            kepessegekListBox.Items.Clear();
            
        }

        public void AdatfrissitLeiras() {

            kepessegekLeirasatextBox.Clear();
            string text = kepessegekListBox.GetItemText(kepessegekListBox.SelectedItem);
            var command = conn.CreateCommand();
            command.CommandText = @"
                                SELECT leiras
                                FROM kepessegek 
                                WHERE nev 
                                LIKE @active
                                ";
            command.Parameters.AddWithValue("@active", text);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string leiras = reader.GetString("leiras");
                    string sor = string.Format("{0}", leiras);
                    kepessegekLeirasatextBox.Text = sor;
                }
            }
        }

        public void buttonHozzaad_Click(object sender, EventArgs e)
        {
            string kepessegmegnevezese = kepessegNeveTextBox.Text.ToString();
            string kepessegLeirasa = leirasTextBox.Text.ToString();
            string kepesseghasznalo = hasznaloComboBox.Text.ToString();
            var command = conn.CreateCommand();
            command.CommandText = @"
                                INSERT INTO 
                                `cs_harcosok`.`kepessegek` 
                                (
                                `nev`,
                                `leiras`,
                                `harcos_id`
                                )
                                VALUES
                                (
                                @nev,
                                @leiras,
                                (SELECT id FROM harcosok WHERE nev = @harcos_id)
                                );
                                ";
            command.Parameters.AddWithValue("@nev", kepessegmegnevezese);
            command.Parameters.AddWithValue("@leiras", kepessegLeirasa);
            command.Parameters.AddWithValue("@harcos_id", kepesseghasznalo);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySql.Data.MySqlClient.MySqlException eee)
            {

                MessageBox.Show("Hiba");
            }
            AdatFrissit();
        }

        private void kepessegekListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AdatfrissitLeiras();
        }

        private void harcosokListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AdatFrissitKepessegei();
        }
    }
}
