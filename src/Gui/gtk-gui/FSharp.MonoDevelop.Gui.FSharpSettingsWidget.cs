
// This file has been generated by the GUI designer. Do not modify.
namespace FSharp.MonoDevelop.Gui
{
	public partial class FSharpSettingsWidget
	{
		private global::Gtk.VBox vbox1;
		private global::Gtk.Frame frame3;
		private global::Gtk.Alignment GtkAlignment;
		private global::Gtk.Table table1;
		private global::Gtk.Button buttonBrowse;
		private global::Gtk.Entry entryArguments;
		private global::Gtk.Entry entryPath;
		private global::Gtk.Label GtkLabel1;
		private global::Gtk.Label GtkLabel6;
		private global::Gtk.Label GtkLabel2;
		private global::Gtk.Frame frame4;
		private global::Gtk.Alignment GtkAlignment1;
		private global::Gtk.FontButton fontbutton1;
		private global::Gtk.Label GtkLabel13;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget FSharp.MonoDevelop.Gui.FSharpSettingsWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "FSharp.MonoDevelop.Gui.FSharpSettingsWidget";
			// Container child FSharp.MonoDevelop.Gui.FSharpSettingsWidget.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.frame3 = new global::Gtk.Frame ();
			this.frame3.Name = "frame3";
			this.frame3.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame3.Gtk.Container+ContainerChild
			this.GtkAlignment = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.GtkAlignment.Name = "GtkAlignment";
			this.GtkAlignment.LeftPadding = ((uint)(12));
			this.GtkAlignment.TopPadding = ((uint)(6));
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			this.table1 = new global::Gtk.Table (((uint)(3)), ((uint)(3)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.buttonBrowse = new global::Gtk.Button ();
			this.buttonBrowse.CanFocus = true;
			this.buttonBrowse.Name = "buttonBrowse";
			this.buttonBrowse.UseUnderline = true;
			this.buttonBrowse.Label = global::Mono.Unix.Catalog.GetString ("_Browse...");
			this.table1.Add (this.buttonBrowse);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1 [this.buttonBrowse]));
			w1.LeftAttach = ((uint)(2));
			w1.RightAttach = ((uint)(3));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryArguments = new global::Gtk.Entry ();
			this.entryArguments.CanFocus = true;
			this.entryArguments.Name = "entryArguments";
			this.entryArguments.IsEditable = true;
			this.entryArguments.InvisibleChar = '●';
			this.table1.Add (this.entryArguments);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryArguments]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(3));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryPath = new global::Gtk.Entry ();
			this.entryPath.CanFocus = true;
			this.entryPath.Name = "entryPath";
			this.entryPath.IsEditable = true;
			this.entryPath.InvisibleChar = '●';
			this.table1.Add (this.entryPath);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryPath]));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.GtkLabel1 = new global::Gtk.Label ();
			this.GtkLabel1.Name = "GtkLabel1";
			this.GtkLabel1.Xalign = 0F;
			this.GtkLabel1.LabelProp = global::Mono.Unix.Catalog.GetString ("Path");
			this.GtkLabel1.UseMarkup = true;
			this.table1.Add (this.GtkLabel1);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1 [this.GtkLabel1]));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.GtkLabel6 = new global::Gtk.Label ();
			this.GtkLabel6.Name = "GtkLabel6";
			this.GtkLabel6.Xalign = 0F;
			this.GtkLabel6.LabelProp = global::Mono.Unix.Catalog.GetString ("Options");
			this.GtkLabel6.UseMarkup = true;
			this.table1.Add (this.GtkLabel6);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1 [this.GtkLabel6]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			this.GtkAlignment.Add (this.table1);
			this.frame3.Add (this.GtkAlignment);
			this.GtkLabel2 = new global::Gtk.Label ();
			this.GtkLabel2.Name = "GtkLabel2";
			this.GtkLabel2.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>F# Interactive</b>");
			this.GtkLabel2.UseMarkup = true;
			this.frame3.LabelWidget = this.GtkLabel2;
			this.vbox1.Add (this.frame3);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.frame3]));
			w8.Position = 0;
			// Container child vbox1.Gtk.Box+BoxChild
			this.frame4 = new global::Gtk.Frame ();
			this.frame4.Name = "frame4";
			this.frame4.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child frame4.Gtk.Container+ContainerChild
			this.GtkAlignment1 = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.GtkAlignment1.Name = "GtkAlignment1";
			this.GtkAlignment1.LeftPadding = ((uint)(12));
			this.GtkAlignment1.TopPadding = ((uint)(6));
			// Container child GtkAlignment1.Gtk.Container+ContainerChild
			this.fontbutton1 = new global::Gtk.FontButton ();
			this.fontbutton1.CanFocus = true;
			this.fontbutton1.Name = "fontbutton1";
			this.GtkAlignment1.Add (this.fontbutton1);
			this.frame4.Add (this.GtkAlignment1);
			this.GtkLabel13 = new global::Gtk.Label ();
			this.GtkLabel13.Name = "GtkLabel13";
			this.GtkLabel13.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Interactive Pad Font</b>");
			this.GtkLabel13.UseMarkup = true;
			this.frame4.LabelWidget = this.GtkLabel13;
			this.vbox1.Add (this.frame4);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.frame4]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
