using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace WYD_strdef_editor {
	public partial class MainWindow : Window {
		// Atributos
		private string FileName { get; set; } = null;

		private byte [ ] FileBytes { get; set; } = null;

		private string [ ] FileContent { get; set; } = null;

		// Construtor
		public MainWindow ( ) {
			try {
				this.InitializeComponent ( );

				this.ContentRendered += ( a , b ) => this.OnRender ( );

				this.UI_List.SelectionChanged += ( a , b ) => this.OnItemChange ( );

				this.UI_EDIT.TextChanged += ( a , b ) => this.UpdateLength ( );

				this.UI_CANCEL.Click += ( a , b ) => this.CancelEdit ( );
				this.UI_SAVE.Click += ( a , b ) => this.SaveEdit ( );
			}
			catch ( Exception ex ) { this.Error ( ex ); }
		}

		// Alertas de erro
		public void Error ( object err ) => MessageBox.Show ( err.ToString ( ) , "Erro !!!" , MessageBoxButton.OK , MessageBoxImage.Error );

		// Ao renderizar a janela
		private void OnRender ( ) {
			try {
				OpenFileDialog find = new OpenFileDialog {
					Filter = "strdef.bin|strdef.bin" ,
					Title = "Selecione o seu strdef.bin"
				};

				find.ShowDialog ( );

				if ( find.CheckFileExists && File.Exists ( find.FileName ) ) {
					this.FileName = find.FileName;

					this.UpdateAll ( );

					this.OnItemChange ( );
				}
				else {
					Environment.Exit ( 0 );
				}
			}
			catch ( Exception ex ) { this.Error ( ex ); }
		}

		// Atualiza tudo
		private void UpdateAll ( ) {
			try {
				if ( this.FileName == null ) {
					throw new Exception ( "this.FileName == null" );
				}
				else {
					this.FileBytes = File.ReadAllBytes ( this.FileName );

					for ( int i = 0 ; i < this.FileBytes.Length ; i++ ) {
						this.FileBytes [ i ] ^= 0x5A;
					}

					int total = ( int ) Math.Floor ( this.FileBytes.Length / 128d );

					this.FileContent = new string [ total ];

					this.UI_List.Items.Clear ( );

					for ( int i = 0 ; i < total ; i++ ) {
						this.FileContent [ i ] = Encoding.UTF7.GetString ( this.FileBytes , i * 128 , 128 ).Replace ( "\0" , "" );

						this.UI_List.Items.Add ( $"{i}: {this.FileContent [ i ]}" );
					}
				}
			}
			catch ( Exception ex ) { this.Error ( ex ); }
		}

		// Ao alterar item selecionado
		private void OnItemChange ( ) {
			try {
				int selected = this.UI_List.SelectedIndex;

				this.UI_EDIT.IsEnabled = this.UI_CANCEL.IsEnabled = this.UI_SAVE.IsEnabled = selected >= 0;

				this.UI_EDIT.Text = this.UI_EDIT.IsEnabled ? this.FileContent [ selected ] : "";

				if ( this.UI_EDIT.IsEnabled ) {
					this.UI_List.ScrollIntoView ( this.UI_List.Items [ selected ] );
				}

				this.UpdateLength ( );
			}
			catch ( Exception ex ) { this.Error ( ex ); }
		}

		// Atualiza tamanho
		private void UpdateLength ( ) {
			try {
				this.UI_LENGTH.Content = $"{128 - this.UI_EDIT.Text.Length}";
			}
			catch ( Exception ex ) { this.Error ( ex ); }
		}

		// Cancela edição
		private void CancelEdit ( ) {
			try {
				int selected = this.UI_List.SelectedIndex;

				if ( selected >= 0 ) {
					this.UI_EDIT.Text = this.FileContent [ selected ];
				}
			}
			catch ( Exception ex ) { this.Error ( ex ); }
		}

		// Salva edição
		private void SaveEdit ( ) {
			try {
				int selected = this.UI_List.SelectedIndex;

				if ( selected >= 0 ) {
					byte [ ] data = Encoding.UTF7.GetBytes ( this.UI_EDIT.Text );

					Array.Resize ( ref data , 128 );

					byte [ ] buffer = this.FileBytes.ToArray ( );

					Array.Copy ( data , 0 , buffer , selected * 128 , 128 );

					for ( int i = 0 ; i < buffer.Length ; i++ ) {
						buffer [ i ] ^= 0x5A;
					}

					File.WriteAllBytes ( this.FileName , buffer );

					this.UpdateAll ( );

					this.UI_List.SelectedIndex = selected;
					this.UI_List.ScrollIntoView ( this.UI_List.Items [ selected ] );
				}
			}
			catch ( Exception ex ) { this.Error ( ex ); }
		}
	}
}