﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magic_Shop
{
	public partial class MainWindow : Form
	{
		Names myNames = new Names();
		private DataSet itemsDS;
		Random rndQty = new Random();
		string windowTitle;
		Random rndPrice = new Random();
		//save the original price list into an array for the random price functionality
		List<decimal> origStorePriceList = new List<decimal>();
		public static string sFileName;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Form_Load(object sender, EventArgs e)
		{
			windowTitle = this.Text;
			itemsDS = new DataSet("ItemDataSet");
			//attempt to load XML path on form load, doesn't work, don't know why
			//itemsDS.ReadXml(XMLfilePath);

			//attempt to turn off autogenerating columns, doesn't work, don't know why
			//storeGridView.AutoGenerateColumns = false;
			//storeGridView.DataSource = ItemDataSet;
			// storeGridView.DataMember = "item";
			//DataGridViewLinkColumn itemColumn = new DataGridViewLinkColumn();
			//storeGridView.AllowUserToAddRows = false;
			// scGridView.AllowUserToAddRows = false;
			//scGridView.AutoGenerateColumns = false;
			disabledPanel.Visible = true;
			disabledPanel.BringToFront();
			disabledPanel2.Visible = true;
			disabledPanel2.BringToFront();
			//preselect the average pricing level
			//shopPricingLevel.SelectedIndex = 1;

		}

		private string GetItemRarity(int RowNum)
		{
			//get item rarity by looking in column named 'storeRarityCol' for that cell
			string itemRarity = storeGridView.Rows[RowNum].Cells["storeRarityCol"].Value.ToString();
			return itemRarity;
		}

		private int SetItemQuantity(string Rarity)
		{


			if (Rarity == "Common")
			{
				int Qty = rndQty.Next(5, 11);
				return Qty;
			}

			else if (Rarity == "Uncommon")
			{

				int Qty = rndQty.Next(0, 6);
				return Qty;
			}
			else if (Rarity == "Rare")
			{
				int Qty = rndQty.Next(0, 2);
				return Qty;

			}
			else if (Rarity == "Very Rare")
			{
				int dropRate = rndQty.Next(1, 11);
				if (dropRate == 10)
				{
					return 1;

				}
				else return 0;

			}
			else if (Rarity == "Legendary")
			{
				int dropRate = rndQty.Next(1, 51);
				if (dropRate == 50)
				{
					return 1;
				}
				else return 0;
			}
			else
				return 0;
		}

		public void ReadXML()
		{
			OpenFileDialog openXMLDialog = new OpenFileDialog();
			openXMLDialog.Filter = "XML File | *.xml";
			openXMLDialog.Title = "Select a(n) XML file with a list of items";
			ItemDataSet.Clear();
			if (openXMLDialog.ShowDialog() == DialogResult.OK)
			{
				sFileName = openXMLDialog.FileName;


				this.Text = windowTitle + " - " + sFileName;
				try
				{
					ItemDataSet.ReadXml(sFileName);
				}
				catch (Exception e)
				{
					MessageBox.Show("Problem reading XML file, probably malformed. \n Details: \n" + e);
				}
				storeGridView.AutoGenerateColumns = false;
				storeGridView.DataSource = ItemDataSet;
				storeGridView.DataMember = "item";
				scGridView.AllowUserToAddRows = false;
				scGridView.AutoGenerateColumns = false;

				//set all store item quantities to 0
				for (int row = 0; row < storeGridView.Rows.Count; row++)
				{
					storeGridView.Rows[row].Cells["storeQuantityCol"].Value = 0;
				}
				//enable functionality
				calculateButton.Enabled = true;
				scGridView.Enabled = true;
				storeGridView.Enabled = true;
				//hide the instructional elements
				disabledLabel1.Enabled = false;
				disabledLabel1.Text = "";
				//hide the panels covering functionality
				disabledPanel.Visible = false;
				disabledLabel4.Visible = true;
				arrow1label.Visible = false;
				arrow2label.Visible = true;
				generateToolStripMenuItem.Enabled = true;


				int storeRowCount = storeGridView.RowCount;
				for (int i = 0; i < storeRowCount; i++)
				{
					origStorePriceList.Add(Convert.ToDecimal(storeGridView.Rows[i].Cells["storePriceCol"].Value.ToString()));
				}

				this.Activate();


			}

		}

		public DataGridViewRow CloneWithValues(DataGridViewRow row)
		{
			DataGridViewRow clonedRow = (DataGridViewRow)row.Clone();
			//index starts at 2 to avoid copying the buy/return and qty columns
			for (Int32 i = 2; i < row.Cells.Count; i++)
			{
				clonedRow.Cells[i].Value = row.Cells[i].Value;
			}
			return clonedRow;
		}

		public void BuyFirstItem(int storeItemRowNum)
		{
			//create a new temporary row that clones the values from the store (not including the the first 3 values)
			DataGridViewRow tempRow = (DataGridViewRow)CloneWithValues(storeGridView.Rows[storeItemRowNum]);
			tempRow.Cells[1].Value = 1;
			scGridView.Rows.Add(tempRow);
			return;
		}

		public void HideRowIfZero(int itemRowNum)
		{
			if ((int)storeGridView.Rows[itemRowNum].Cells["storeQuantityCol"].Value <= 0)
			{
				storeGridView.CurrentCell = null;
				storeGridView.Rows[itemRowNum].Visible = false;
			}
		}


		private void BuyItemFromStore(int storeItemRowNum)
		{
			//find the name of the item in that row
			string storeItemName = (string)storeGridView.Rows[storeItemRowNum].Cells["storeItemNameCol"].Value;
			//find the qty of the item in that row
			int storeItemQty = (int)storeGridView.Rows[storeItemRowNum].Cells["storeQuantityCol"].Value;

			if (storeItemQty == 0)
			{
				System.Windows.Forms.MessageBox.Show("Please \"Generate Shop\" first before buying items. \n This is just to show that the items XML file loaded properly.");
				return;
			}

			//decrement the qty of the item
			storeItemQty--;
			//update the qty in the row

			storeGridView.Rows[storeItemRowNum].Cells["storeQuantityCol"].Value = storeItemQty;

			if (scGridView.RowCount == 0) //if shopping cart is empty
			{
				BuyFirstItem(storeItemRowNum);
				HideRowIfZero(storeItemRowNum);
				calculateButton.PerformClick();
				return;
			}
			else //shopping cart not empty
			{
				//iterate through the table
				//initialize the index where a duplicate MIGHT be found
				int cartItemDuplicateIndex = -1;
				for (int i = 0; i < scGridView.Rows.Count; i++)
				{
					if (storeItemName.Equals(scGridView.Rows[i].Cells["scItemNameCol"].Value))
					{
						//use var to find the index of the possible dupe
						cartItemDuplicateIndex = i;
					}
				}
				if (cartItemDuplicateIndex > -1) //cart not empty, item is a dupe
				{
					//add 1 to the qty of cartItemDuplicateIndex's qty
					int DupeItemQty = (int)scGridView.Rows[cartItemDuplicateIndex].Cells["scQuantityCol"].Value;
					DupeItemQty++;
					scGridView.Rows[cartItemDuplicateIndex].Cells["scQuantityCol"].Value = DupeItemQty;
					HideRowIfZero(storeItemRowNum);
					calculateButton.PerformClick();
					return;
				}
				//shopping cart not empty but did not find duplicate

				BuyFirstItem(storeItemRowNum);
				HideRowIfZero(storeItemRowNum);
				calculateButton.PerformClick();
				return;
			}

		}
		private void ReturnItemToStore(int scItemRowNum)
		{
			//find the name of the item in the store's item row
			string scItemName = (string)scGridView.Rows[scItemRowNum].Cells["scItemNameCol"].Value;
			//find qty of of the item in the store's item row
			int scItemQty = (int)scGridView.Rows[scItemRowNum].Cells["scQuantityCol"].Value;
			//decrement qty
			scItemQty--;
			//update qty
			scGridView.Rows[scItemRowNum].Cells["scQuantityCol"].Value = scItemQty;
			//HideRowIfZero(scItemRowNum);

			//initialize dupe check index
			int storeDupeIndex = -1;
			//iterate through the store to look for duplicates
			for (int i = 0; i < storeGridView.Rows.Count; i++)
			{
				if (scItemName.Equals(storeGridView.Rows[i].Cells["storeItemNameCol"].Value))
				{
					storeDupeIndex = i;
				}
			}

			if (storeDupeIndex > -1)
			{
				//add 1 to the qty of storeDupeIndex's qty
				int storeDupeQty = (int)storeGridView.Rows[storeDupeIndex].Cells["storeQuantityCol"].Value;
				storeDupeQty++;
				storeGridView.Rows[storeDupeIndex].Cells["storeQuantityCol"].Value = storeDupeQty;
				storeGridView.Rows[storeDupeIndex].Visible = true;
				calculateButton.PerformClick();

			}
			else //returned item wasn't found in the store...
				 //how'd you get here?
				return;
			if ((int)scGridView.Rows[scItemRowNum].Cells["scQuantityCol"].Value <= 0)
			{
				scGridView.CurrentCell = null;
				scGridView.Rows.RemoveAt(scItemRowNum);
				calculateButton.PerformClick();

			}
		}
		//clicking on store button or item name for descript method
		private void storeGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0)
				return;

			//check if clicking on the proper column for item name, if not, don't do anything
			string clickedColName = storeGridView.Columns[e.ColumnIndex].Name;
			if (clickedColName == "storeItemNameCol")
			{
				ItemDetails ItemDetailsPop = new ItemDetails();
				ItemDetailsPop.itemnametext.Text = storeGridView.Rows[e.RowIndex].Cells["storeItemNameCol"].Value.ToString();
				ItemDetailsPop.itemtypetext.Text = storeGridView.Rows[e.RowIndex].Cells["storeTypeCol"].Value.ToString();
				ItemDetailsPop.itemraritytext.Text = storeGridView.Rows[e.RowIndex].Cells["storeRarityCol"].Value.ToString();
				ItemDetailsPop.itemweighttext.Text = storeGridView.Rows[e.RowIndex].Cells["storeWeightCol"].Value.ToString();
				ItemDetailsPop.itempricetext.Text = storeGridView.Rows[e.RowIndex].Cells["storePriceCol"].Value.ToString();
				ItemDetailsPop.itemreftext.Text = storeGridView.Rows[e.RowIndex].Cells["storeRefCol"].Value.ToString();
				ItemDetailsPop.itemdescriptext.Text = storeGridView.Rows[e.RowIndex].Cells["storeDescriptionCol"].Value.ToString();
				ItemDetailsPop.ShowDialog();
			}
			if (clickedColName == "storeBuyCol")
			{
				int storeItemRowNum = storeGridView.CurrentCell.RowIndex;
				BuyItemFromStore(storeItemRowNum);
			}

			else
				return;

		}

		public void randomizeQty()
		{
			//ideally, want this button to go row by row:
			// 1. get the rarity from the item
			// 2. generate a random quantity from the rarity
			// 3. set the quantity in the first column to that random quantity
			int numOfRows = storeGridView.RowCount;
			for (int i = 0; i < numOfRows; i++)
			{
				storeGridView.Rows[i].Visible = true;
				string itemRarity = GetItemRarity(i);
				int itemQuantity = SetItemQuantity(itemRarity);
				storeGridView.Rows[i].Cells["storeQuantityCol"].Value = itemQuantity;
				if (itemQuantity == 0)
				{
					storeGridView.CurrentCell = null;
					storeGridView.Rows[i].Visible = false;

				}
			}
			disabledLabel4.Visible = false;
			disabledPanel2.Visible = false;
			arrow2label.Visible = false;
		}

		//clicking on sc return or item name for descript method
		private void scGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			string clickedColName = scGridView.Columns[e.ColumnIndex].Name;

			if (e.RowIndex < 0)
				return;
			else if (clickedColName == "scItemNameCol")
			{
				//build strings for the item details pop up

				ItemDetails ItemDetailsPop = new ItemDetails();
				ItemDetailsPop.itemnametext.Text = scGridView.Rows[e.RowIndex].Cells["scItemNameCol"].Value.ToString();
				ItemDetailsPop.itemtypetext.Text = scGridView.Rows[e.RowIndex].Cells["scTypeCol"].Value.ToString();
				ItemDetailsPop.itemraritytext.Text = scGridView.Rows[e.RowIndex].Cells["scRarityCol"].Value.ToString();
				ItemDetailsPop.itemweighttext.Text = scGridView.Rows[e.RowIndex].Cells["scWeightCol"].Value.ToString();
				ItemDetailsPop.itempricetext.Text = scGridView.Rows[e.RowIndex].Cells["scPriceCol"].Value.ToString();
				ItemDetailsPop.itemreftext.Text = scGridView.Rows[e.RowIndex].Cells["scRefCol"].Value.ToString();
				ItemDetailsPop.itemdescriptext.Text = scGridView.Rows[e.RowIndex].Cells["scDescriptionCol"].Value.ToString();
				ItemDetailsPop.ShowDialog();

			}
			else if (clickedColName == "scReturnCol")
			{
				ReturnItemToStore(e.RowIndex);
			}

			else
				return;

		}

		private void calculateButton_Click(object sender, EventArgs e)
		{
			decimal totalInCart = 0;
			int numOfSCRows = scGridView.RowCount;
			if (numOfSCRows > 0)
			{
				for (int i = 0; i < numOfSCRows; i++)
				{
					decimal priceInRow = Convert.ToDecimal(scGridView.Rows[i].Cells["scPriceCol"].Value.ToString());
					int qtyInRow = Int32.Parse(scGridView.Rows[i].Cells["scQuantityCol"].Value.ToString());
					decimal totalInRow = priceInRow * qtyInRow;
					totalInCart += totalInRow;
				}
				scTotalTextBox.Text = totalInCart.ToString();
				try
				{
					decimal endingGP = Int32.Parse(startingGPTextBox.Text) - totalInCart;
					endingGPTextBox.Text = endingGP.ToString();
					return;
				}
				catch (FormatException f)
				{
					System.Windows.Forms.MessageBox.Show("Please enter a number for \"Starting GP\"!:\n\n" + f);

				}

				return;
			}
			return;

		}

		//go to github page when clicked
		private void aboutLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/garylzimmer/Magic-Shop/");
		}

		// below causes too many pop ups
		//whenever a row is added to the shopping cart, recalculate
		//private void scGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		//{
		//    calculateButton.PerformClick();
		// }
		//whenever a row is removed from the shopping cart, recalculate
		//private void scGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		//{
		//    calculateButton.PerformClick();
		//}

		private void readXMLFileMenuItem_Click(object sender, EventArgs e)
		{
			ReadXML();
		}


		private void aboutMenuItem_Click(object sender, EventArgs e)
		{
			//MessageBox.Show("Magic Shop \n version 1.1 \n by Gary Zimmer \n https://github.com/garylzimmer/Magic-Shop/");
			About myAbout = new About();
			myAbout.ShowDialog();

		}

		private void editXMLFileMenuItem_Click(object sender, EventArgs e)
		{

		}

		private decimal randomizeItemPrice(int rownum)
		{
			string itemRarity = GetItemRarity(rownum);
			decimal priceMultiplier = 0;
			decimal itemPrice = origStorePriceList[rownum];
			switch (shopPricingLevel.Text)
			{
				case "Cheap":
					priceMultiplier = rndPrice.Next(50, 101);
					break;
				case "Average":
					priceMultiplier = rndPrice.Next(75, 126);
					break;
				case "Expensive":
					priceMultiplier = rndPrice.Next(115, 156);
					break;
				default:
					priceMultiplier = 100M;
					break;
			}
			//(priceMultiplier/100)*itemPrice
			return decimal.Multiply(decimal.Divide(priceMultiplier, 100m), itemPrice);
		}


		private void randomizeStorePrices()
		{
			if (randomizePricesCheckBox.Checked == true)
			{
				int storeRowCount = storeGridView.RowCount;
				for (int i = 0; i < storeRowCount; i++)
				{
					storeGridView.Rows[i].Cells["storePriceCol"].Value = randomizeItemPrice(i);
				}
			}
			else
			{
				for (int i = 0; i < storeGridView.RowCount; i++)
				{
					storeGridView.Rows[i].Cells["storePriceCol"].Value = origStorePriceList[i];
				}
			}
		}

		private void randomizePrices_CheckedChanged(object sender, EventArgs e)
		{
			randomizeStorePrices();
		}

		private void shopPricingLevel_SelectedIndexChanged(object sender, EventArgs e)
		{
			randomizeStorePrices();

		}

		private void openStoreArchetypeXMLFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ReadXML();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void SaveGeneratedStore()
		{
			//go through store and delete non-visible rows
			//actually, why bother?
			

			//save storeDGV as Dataset
			DataSet savedStoreDataSet = (DataSet)storeGridView.DataSource;

			//save Dataset as XML
			//Create the FileStream to write with.
			SaveFileDialog saveXMLDialog = new SaveFileDialog();
			saveXMLDialog.Filter = "XML File | *.xml";
			saveXMLDialog.Title = "Save the Generated Store XML File";
			saveXMLDialog.ShowDialog();

			if (saveXMLDialog.FileName != "")
			{
				try
				{
					System.IO.FileStream stream = new System.IO.FileStream(saveXMLDialog.FileName, System.IO.FileMode.Create);
					savedStoreDataSet.WriteXml(stream);
					stream.Close();
					MessageBox.Show("File Saved.");
					
				}
				catch (Exception ioErr)
				{
					MessageBox.Show("IO Error. You file is already open. Probably. \n" + ioErr);

				}
			}

		}


		private void saveThisGeneratedStoreToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveGeneratedStore();
		}

		private void fileMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void disabledLabel1_Click(object sender, EventArgs e)
		{

		}

		private void storeNameGenButton_Click(object sender, EventArgs e)
		{
			storeNameLabel.Text = myNames.generateRandomStoreName();
		}

		private void storeNameFunnyGenButton_Click(object sender, EventArgs e)
		{
			storeNameLabel.Text = myNames.generateFunnyStoreName();
		}

		private void editXMLFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EditXML myEditXML = new EditXML();
			myEditXML.Show();
		}

		private void newShopInventoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			randomizeQty();
		}

		private void storeInventoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			randomizeQty();
		}

		private void randomStoreNameToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			storeNameLabel.Text = myNames.generateRandomStoreName();

		}

		private void uniqueStoreNameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			storeNameLabel.Text = myNames.generateFunnyStoreName();
		}

		private void disabledLabel4_Click(object sender, EventArgs e)
		{

		}
	}





}
