using System;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using Gtk;

namespace workwear
{
	public partial class Nomenclature : Gtk.Dialog
	{
		public bool NewItem;
		int Itemid;
		
		public Nomenclature()
		{
			this.Build();
			ComboWorks.ComboFillReference(comboUnits, "units", 2);
			ComboWorks.ComboFillReference(comboType, "item_types", 2);
			ComboWorks.ComboFillUniqueValue(comboentrySize, "nomenclature", "size");
			ComboWorks.ComboFillUniqueValue(comboentryGrowth, "nomenclature", "growth");
		}

		public void Fill(int id)
		{
			Itemid = id;
			NewItem = false;
			
			MainClass.StatusMessage(String.Format("Запрос номенклатуры №{0}...", id));
			string sql = "SELECT * FROM nomenclature WHERE nomenclature.id = @id";
			try
			{
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", id);
				
				using(MySqlDataReader rdr = cmd.ExecuteReader())
				{		
					TreeIter iter;
					
					rdr.Read();
					
					entryID.Text = rdr["id"].ToString();
					entryName.Text = rdr["name"].ToString();
					comboentrySize.Entry.Text = rdr["size"].ToString();
					comboentryGrowth.Entry.Text = rdr["growth"].ToString();
					if(rdr["units_id"] != DBNull.Value)
					{
						ListStoreWorks.SearchListStore((ListStore)comboUnits.Model, rdr.GetInt32("units_id"), out iter);
						comboUnits.SetActiveIter(iter);
					}
					if(rdr["type_id"] != DBNull.Value)
					{
						ListStoreWorks.SearchListStore((ListStore)comboType.Model, rdr.GetInt32("type_id"), out iter);
						comboType.SetActiveIter(iter);
					}
					
					MainClass.StatusMessage("Ok");
				}
				this.Title = entryName.Text;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка получения информации о номенклатуре!");
				QSMain.ErrorMessage(this, ex);
				this.Respond(Gtk.ResponseType.Reject);
			}
			TestCanSave();
		}

		protected void TestCanSave ()
		{
			bool Nameok = entryName.Text != "";
			buttonOk.Sensitive = Nameok;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string sql;
			if(NewItem)
			{
				sql = "INSERT INTO nomenclature (name, type_id, units_id, size, growth) " +
					"VALUES (@name, @type_id, @units_id, @size, @growth)";
			}
			else
			{
				sql = "UPDATE nomenclature SET name = @name, type_id = @type_id, units_id = @units_id, " +
				"size = @size, growth = @growth WHERE id = @id";
			}
			MainClass.StatusMessage("Запись номенклатуры...");
			try 
			{
				TreeIter iter;
				MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
				
				cmd.Parameters.AddWithValue("@id", Itemid);
				cmd.Parameters.AddWithValue("@name", entryName.Text);
				if(comboentrySize.Entry.Text != "")
					cmd.Parameters.AddWithValue("@size", comboentrySize.Entry.Text);
				else 
					cmd.Parameters.AddWithValue("@size", DBNull.Value);
				if (comboentryGrowth.Entry.Text != "")
					cmd.Parameters.AddWithValue("@growth", comboentryGrowth.Entry.Text);
				else
					cmd.Parameters.AddWithValue("@growth", DBNull.Value);
				if(comboUnits.Active > 0 && comboUnits.GetActiveIter(out iter))
					cmd.Parameters.AddWithValue("@units_id", comboUnits.Model.GetValue(iter,1));
				else
					cmd.Parameters.AddWithValue("@units_id", DBNull.Value);
				if(comboType.Active > 0 && comboType.GetActiveIter(out iter))
					cmd.Parameters.AddWithValue("@type_id", comboType.Model.GetValue(iter,1));
				else
					cmd.Parameters.AddWithValue("@type_id", DBNull.Value);
				
				cmd.ExecuteNonQuery();
				MainClass.StatusMessage("Ok");
				Respond (Gtk.ResponseType.Ok);
			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.ToString());
				MainClass.StatusMessage("Ошибка записи номенклатуры!");
				QSMain.ErrorMessage(this,ex);
			}
		}

		protected void OnEntryNameChanged(object sender, EventArgs e)
		{
			TestCanSave();
		}
	}
}

