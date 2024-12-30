using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Microsoft.Data.Sqlite; // from the Microsoft.Data.Sqlite NuGet package


using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Diagnostics;

namespace MauiRfidSample
{

    public partial class SQLiteInterface
    {
        // Adjust this to your actual connection string
        static string dbPath = Path.Combine(FileSystem.AppDataDirectory, "item.db");
        public string _ConnectionString = $"Data Source={dbPath};Version=3;";


        #region "Helper Methods (inline replacements for GlobalHelpers)"

        private string SafeString(object obj)
        {
            if (obj == null || obj == DBNull.Value) return string.Empty;
            return obj.ToString();
        }

        private int SafeInt(object obj)
        {
            if (obj == null || obj == DBNull.Value) return 0;
            if (int.TryParse(obj.ToString(), out int val))
                return val;
            return 0;
        }

        private bool SafeBool(object obj)
        {
            // Example: treat nonzero int as true, or parse "True"/"False" string
            if (obj == null || obj == DBNull.Value) return false;
            if (bool.TryParse(obj.ToString(), out bool b))
            {
                return b;
            }
            else if (int.TryParse(obj.ToString(), out int i))
            {
                return i != 0;
            }
            return false;
        }

        private void LogError(Exception ex)
        {
            // Replace with your own MAUI-friendly logging if you prefer.
            Console.WriteLine(ex.ToString());
        }

        #endregion

        // drop the Items table
        public bool DropItemsTable()
        {
            bool bSuccess = false;

            try
            {
                using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                {
                    sqlConn.Open();

                    StringBuilder sbSQL = new StringBuilder(128);
                    sbSQL.Append("DROP TABLE tblItems");

                    using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                    {
                        sqlCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return bSuccess;
        }

        // initialize the Items table
        public bool CreateItemsTable()
        {
            bool bSuccess = false;

            try
            {
                using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                {
                    sqlConn.Open();

                    // 1) CREATE TABLE
                    StringBuilder sbSQL = new StringBuilder(128);
                    sbSQL.Append("CREATE TABLE tblItems ");
                    sbSQL.Append(" (");
                    sbSQL.Append(" [ID] TEXT PRIMARY KEY NOT NULL COLLATE NOCASE, ");
                    sbSQL.Append(" [Name] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [Description] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [ItemGroupID] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [DefaultLocationID] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [ItemURL] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [Color] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [Style] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [Material] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [UPC] TEXT NULL COLLATE NOCASE DEFAULT '', ");
                    sbSQL.Append(" [TagCount] NUMBER NULL DEFAULT 0, ");
                    sbSQL.Append(" [UseForRFID] INTEGER NULL DEFAULT 0, ");
                    sbSQL.Append(" [IsPhantomBOM] INTEGER NULL DEFAULT 0, ");
                    sbSQL.Append(" [RFIDItemReference] TEXT NULL COLLATE NOCASE DEFAULT '' ");
                    sbSQL.Append(" )");

                    using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                    {
                        sqlCmd.ExecuteNonQuery();
                    }

                    // 2) CREATE INDEXES
                    sbSQL = new StringBuilder(128);
                    sbSQL.Append("CREATE INDEX ixColor ON tblItems([Color]); ");
                    sbSQL.Append("CREATE INDEX ixStyle ON tblItems([Style]); ");
                    sbSQL.Append("CREATE INDEX ixMaterial ON tblItems([Material]); ");
                    sbSQL.Append("CREATE INDEX ixDescription ON tblItems([Description]); ");

                    using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                    {
                        sqlCmd.ExecuteNonQuery();
                    }

                    bSuccess = true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return bSuccess;
        }

        // populate the Items table
        public bool AddToItemsTable(MauiRfidSample.Items.Item[] Items)
        {
            bool bSuccess = false;

            try
            {
                using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                {
                    sqlConn.Open();
                    SqliteTransaction sqlTrans = sqlConn.BeginTransaction();
                    try
                    {
                        StringBuilder sbSQL = new StringBuilder(128);
                        sbSQL.Append("INSERT INTO tblItems ");
                        sbSQL.Append(" ([ID], [Name], [Description], [ItemGroupID], [DefaultLocationID],");
                        sbSQL.Append("  [ItemURL], [Color], [Style], [Material], [UPC], [TagCount], [UseForRFID], [IsPhantomBOM], [RFIDItemReference]) ");
                        sbSQL.Append(" VALUES (@ID, @Name, @Description, @ItemGroupID, @DefaultLocationID,");
                        sbSQL.Append("  @ItemURL, @Color, @Style, @Material, @UPC, @TagCount, @UseForRFID, @IsPhantomBOM, @RFIDItemReference) ");

                        using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                        {
                            // create parameters
                            var pID = sqlCmd.Parameters.Add("@ID", SqliteType.Text);
                            sqlCmd.Parameters.Add("@Name", SqliteType.Text);
                            sqlCmd.Parameters.Add("@Description", SqliteType.Text);
                            sqlCmd.Parameters.Add("@ItemGroupID", SqliteType.Text);
                            sqlCmd.Parameters.Add("@DefaultLocationID", SqliteType.Text);
                            sqlCmd.Parameters.Add("@ItemURL", SqliteType.Text);
                            sqlCmd.Parameters.Add("@Color", SqliteType.Text);
                            sqlCmd.Parameters.Add("@Style", SqliteType.Text);
                            sqlCmd.Parameters.Add("@Material", SqliteType.Text);
                            sqlCmd.Parameters.Add("@UPC", SqliteType.Text);
                            sqlCmd.Parameters.Add("@TagCount", SqliteType.Text);
                            sqlCmd.Parameters.Add("@UseForRFID", SqliteType.Integer);
                            sqlCmd.Parameters.Add("@IsPhantomBOM", SqliteType.Integer);
                            sqlCmd.Parameters.Add("@RFIDItemReference", SqliteType.Integer);

                            // iterate
                            foreach (var item in Items)
                            {
                                sqlCmd.Parameters["@ID"].Value = item.ID;
                                sqlCmd.Parameters["@Name"].Value = item.Name;
                                sqlCmd.Parameters["@Description"].Value = item.Description;
                                sqlCmd.Parameters["@ItemGroupID"].Value = item.ItemGroupID;
                                sqlCmd.Parameters["@DefaultLocationID"].Value = item.DefaultLocationID;
                                sqlCmd.Parameters["@ItemURL"].Value = item.ItemURL;
                                sqlCmd.Parameters["@Color"].Value = item.Color;
                                sqlCmd.Parameters["@Style"].Value = item.Style;
                                sqlCmd.Parameters["@Material"].Value = item.Material;
                                sqlCmd.Parameters["@UPC"].Value = item.UPC;
                                sqlCmd.Parameters["@TagCount"].Value = item.TagCount;
                                sqlCmd.Parameters["@UseForRFID"].Value = item.UseForRFID ? 1 : 0;
                                // Example condition for Phantom BOM
                                sqlCmd.Parameters["@IsPhantomBOM"].Value = (item.BOMType == "BOM-Phantom") ? 1 : 0;
                                sqlCmd.Parameters["@RFIDItemReference"].Value = item.RFIDItemReference;

                                sqlCmd.ExecuteNonQuery();
                            }

                            bSuccess = true;
                        }

                        sqlTrans.Commit();
                    }
                    catch (Exception ex)
                    {
                        sqlTrans.Rollback();
                        LogError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return bSuccess;
        }

        // get the Items that match the selected item group or any of its child groups
        public MauiRfidSample.Items.Item[] GetItems(
            string strItemGroupID,
            string strColor,
            string strStyle,
            string strMaterial,
            string strDescriptionPartial,
            bool bRequireUseForRFID,
            bool bExcludePhantomBOM)
        {
            MauiRfidSample.Items.Item[] Items = new MauiRfidSample.Items.Item[0];

            try
            {
                // at least one filter must be non-empty
                if (!string.IsNullOrWhiteSpace(strItemGroupID) ||
                    !string.IsNullOrWhiteSpace(strColor) ||
                    !string.IsNullOrWhiteSpace(strStyle) ||
                    !string.IsNullOrWhiteSpace(strMaterial) ||
                    !string.IsNullOrWhiteSpace(strDescriptionPartial))
                {
                    using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                    {
                        sqlConn.Open();

                        StringBuilder sbSQL = new StringBuilder(128);
                        sbSQL.Append("SELECT [ID], [Name], [Description], [ItemGroupID], [DefaultLocationID], ");
                        sbSQL.Append(" [ItemURL], [Color], [Style], [Material], [UPC], [TagCount], [UseForRFID], [IsPhantomBOM], [RFIDItemReference] ");
                        sbSQL.Append(" FROM tblItems ");
                        sbSQL.Append(" WHERE [ID] <> '' ");

                        // item group
                        if (!string.IsNullOrWhiteSpace(strItemGroupID))
                        {
                            sbSQL.Append(" AND (");
                            sbSQL.Append("     COALESCE([ItemGroupID], '') = @ItemGroupID ");
                            sbSQL.Append("   OR ");
                            sbSQL.Append("     COALESCE([ItemGroupID], '') IN (SELECT [ID] FROM tblItemGroups WHERE [ParentGroupID] = @ItemGroupID) ");
                            sbSQL.Append(" ) ");
                        }
                        // color
                        if (!string.IsNullOrWhiteSpace(strColor))
                            sbSQL.Append(" AND [Color] = @Color ");
                        // style
                        if (!string.IsNullOrWhiteSpace(strStyle))
                            sbSQL.Append(" AND [Style] = @Style ");
                        // material
                        if (!string.IsNullOrWhiteSpace(strMaterial))
                            sbSQL.Append(" AND [Material] = @Material ");
                        // description partial
                        if (!string.IsNullOrWhiteSpace(strDescriptionPartial))
                            sbSQL.Append(" AND [Description] LIKE @DescriptionPartial ");
                        // require RFID
                        if (bRequireUseForRFID)
                            sbSQL.Append(" AND COALESCE([UseForRFID], 0) <> 0 ");
                        // exclude Phantom BOM
                        if (bExcludePhantomBOM)
                            sbSQL.Append(" AND COALESCE([IsPhantomBOM], 0) = 0 ");

                        // order
                        sbSQL.Append(" ORDER BY [Name] ");

                        using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                        {
                            sqlCmd.Parameters.AddWithValue("@ItemGroupID", strItemGroupID ?? (object)DBNull.Value);
                            sqlCmd.Parameters.AddWithValue("@Color", strColor ?? (object)DBNull.Value);
                            sqlCmd.Parameters.AddWithValue("@Style", strStyle ?? (object)DBNull.Value);
                            sqlCmd.Parameters.AddWithValue("@Material", strMaterial ?? (object)DBNull.Value);
                            sqlCmd.Parameters.AddWithValue("@DescriptionPartial", "%" + strDescriptionPartial + "%");

                            DataTable dtData = new DataTable("Data");
                            using (var reader = sqlCmd.ExecuteReader())
                            {
                                dtData.Load(reader);
                            }

                            Items = new MauiRfidSample.Items.Item[dtData.Rows.Count];
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                DataRow dr = dtData.Rows[i];
                                var item = new MauiRfidSample.Items.Item
                                {
                                    ID = SafeString(dr["ID"]),
                                    Name = SafeString(dr["Name"]),
                                    Description = SafeString(dr["Description"]),
                                    ItemGroupID = SafeString(dr["ItemGroupID"]),
                                    DefaultLocationID = SafeString(dr["DefaultLocationID"]),
                                    ItemURL = SafeString(dr["ItemURL"]),
                                    Color = SafeString(dr["Color"]),
                                    Style = SafeString(dr["Style"]),
                                    Material = SafeString(dr["Material"]),
                                    UPC = SafeString(dr["UPC"]),
                                    TagCount = SafeInt(dr["TagCount"]),
                                    UseForRFID = (SafeInt(dr["UseForRFID"]) != 0),
                                    RFIDItemReference = SafeString(dr["RFIDItemReference"])
                                };
                                Items[i] = item;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            return Items;
        }

        // get a single item by ID
        public MauiRfidSample.Items.Item GetSingleItem(string strItemID)
        {
            var item = new MauiRfidSample.Items.Item();
            try
            {
                using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                {
                    sqlConn.Open();

                    StringBuilder sbSQL = new StringBuilder(128);
                    sbSQL.Append("SELECT [ID], [Name], [Description], [ItemGroupID], [DefaultLocationID], ");
                    sbSQL.Append(" [ItemURL], [Color], [Style], [Material], [UPC], [TagCount], [UseForRFID], [RFIDItemReference] ");
                    sbSQL.Append(" FROM tblItems ");
                    sbSQL.Append(" WHERE [ID] = @ItemID ");

                    using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@ItemID", strItemID);

                        using (SqliteDataReader sqlDR = sqlCmd.ExecuteReader())
                        {
                            if (sqlDR.Read())
                            {
                                item.ID = SafeString(sqlDR["ID"]);
                                item.Name = SafeString(sqlDR["Name"]);
                                item.Description = SafeString(sqlDR["Description"]);
                                item.ItemGroupID = SafeString(sqlDR["ItemGroupID"]);
                                item.DefaultLocationID = SafeString(sqlDR["DefaultLocationID"]);
                                item.ItemURL = SafeString(sqlDR["ItemURL"]);
                                item.Color = SafeString(sqlDR["Color"]);
                                item.Style = SafeString(sqlDR["Style"]);
                                item.Material = SafeString(sqlDR["Material"]);
                                item.UPC = SafeString(sqlDR["UPC"]);
                                item.TagCount = SafeInt(sqlDR["TagCount"]);
                                item.UseForRFID = (SafeInt(sqlDR["UseForRFID"]) != 0);
                                item.RFIDItemReference = SafeString(sqlDR["RFIDItemReference"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            return item;
        }

        // get the distinct colors
        public string[] GetColors(string strItemGroupID)
        {
            List<string> listColors = new List<string>();

            try
            {
                using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                {
                    sqlConn.Open();

                    StringBuilder sbSQL = new StringBuilder(128);
                    sbSQL.Append("SELECT DISTINCT [Color] ");
                    sbSQL.Append(" FROM tblItems ");
                    sbSQL.Append(" WHERE [ID] <> '' ");

                    if (!string.IsNullOrWhiteSpace(strItemGroupID))
                    {
                        sbSQL.Append(" AND (");
                        sbSQL.Append("     COALESCE([ItemGroupID], '') = @ItemGroupID ");
                        sbSQL.Append("   OR ");
                        sbSQL.Append("     COALESCE([ItemGroupID], '') IN (SELECT [ID] FROM tblItemGroups WHERE [ParentGroupID] = @ItemGroupID) ");
                        sbSQL.Append(" ) ");
                    }

                    sbSQL.Append(" ORDER BY [Color] ");

                    using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@ItemGroupID", strItemGroupID ?? (object)DBNull.Value);

                        DataTable dtData = new DataTable("Data");
                        using (var reader = sqlCmd.ExecuteReader())
                        {
                            dtData.Load(reader);
                        }

                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {
                            DataRow dr = dtData.Rows[i];
                            listColors.Add(SafeString(dr["Color"]));
                        }

                        // Insert a blank at the top if not already present
                        if (!listColors.Contains(""))
                            listColors.Insert(0, "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return listColors.ToArray();
        }

        // get the distinct materials
        public string[] GetMaterials(string strItemGroupID)
        {
            List<string> listMaterials = new List<string>();

            try
            {
                using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                {
                    sqlConn.Open();

                    StringBuilder sbSQL = new StringBuilder(128);
                    sbSQL.Append("SELECT DISTINCT [Material] ");
                    sbSQL.Append(" FROM tblItems ");
                    sbSQL.Append(" WHERE [ID] <> '' ");

                    if (!string.IsNullOrWhiteSpace(strItemGroupID))
                    {
                        sbSQL.Append(" AND (");
                        sbSQL.Append("     COALESCE([ItemGroupID], '') = @ItemGroupID ");
                        sbSQL.Append("   OR ");
                        sbSQL.Append("     COALESCE([ItemGroupID], '') IN (SELECT [ID] FROM tblItemGroups WHERE [ParentGroupID] = @ItemGroupID) ");
                        sbSQL.Append(" ) ");
                    }

                    sbSQL.Append(" ORDER BY [Material] ");

                    using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@ItemGroupID", strItemGroupID ?? (object)DBNull.Value);

                        DataTable dtData = new DataTable("Data");
                        using (var reader = sqlCmd.ExecuteReader())
                        {
                            dtData.Load(reader);
                        }

                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {
                            DataRow dr = dtData.Rows[i];
                            listMaterials.Add(SafeString(dr["Material"]));
                        }

                        if (!listMaterials.Contains(""))
                            listMaterials.Insert(0, "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return listMaterials.ToArray();
        }

        // get the distinct styles
        public string[] GetStyles(string strItemGroupID)
        {
            List<string> listStyles = new List<string>();

            try
            {
                using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                {
                    sqlConn.Open();

                    StringBuilder sbSQL = new StringBuilder(128);
                    sbSQL.Append("SELECT DISTINCT [Style] ");
                    sbSQL.Append(" FROM tblItems ");
                    sbSQL.Append(" WHERE [ID] <> '' ");

                    if (!string.IsNullOrWhiteSpace(strItemGroupID))
                    {
                        sbSQL.Append(" AND (");
                        sbSQL.Append("     COALESCE([ItemGroupID], '') = @ItemGroupID ");
                        sbSQL.Append("   OR ");
                        sbSQL.Append("     COALESCE([ItemGroupID], '') IN (SELECT [ID] FROM tblItemGroups WHERE [ParentGroupID] = @ItemGroupID) ");
                        sbSQL.Append(" ) ");
                    }

                    sbSQL.Append(" ORDER BY [Style] ");

                    using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@ItemGroupID", strItemGroupID ?? (object)DBNull.Value);

                        DataTable dtData = new DataTable("Data");
                        using (var reader = sqlCmd.ExecuteReader())
                        {
                            dtData.Load(reader);
                        }

                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {
                            DataRow dr = dtData.Rows[i];
                            listStyles.Add(SafeString(dr["Style"]));
                        }

                        if (!listStyles.Contains(""))
                            listStyles.Insert(0, "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return listStyles.ToArray();
        }

        // get the Items into a dictionary
        public Dictionary<string, MauiRfidSample.Items.Item> GetItemDictionary(bool bRequireUseForRFID)
        {
            Dictionary<string, MauiRfidSample.Items.Item> dcItems = new Dictionary<string, MauiRfidSample.Items.Item>();

            try
            {
                using (SqliteConnection sqlConn = new SqliteConnection(_ConnectionString))
                {
                    sqlConn.Open();

                    StringBuilder sbSQL = new StringBuilder(128);
                    sbSQL.Append("SELECT [ID], [Name], [Description], [ItemGroupID], [DefaultLocationID], ");
                    sbSQL.Append(" [ItemURL], [Color], [Style], [Material], [UPC], [TagCount], [UseForRFID], [RFIDItemReference] ");
                    sbSQL.Append(" FROM tblItems ");
                    sbSQL.Append(" WHERE [ID] <> '' ");

                    if (bRequireUseForRFID)
                        sbSQL.Append(" AND COALESCE([UseForRFID], 0) <> 0 ");

                    sbSQL.Append(" ORDER BY [Name] ");

                    using (SqliteCommand sqlCmd = new SqliteCommand(sbSQL.ToString(), sqlConn))
                    {
                        DataTable dtData = new DataTable("Data");
                        using (var reader = sqlCmd.ExecuteReader())
                        {
                            dtData.Load(reader);
                        }

                        for (int idx = 0; idx < dtData.Rows.Count; idx++)
                        {
                            DataRow dr = dtData.Rows[idx];
                            var item = new MauiRfidSample.Items.Item
                            {
                                ID = SafeString(dr["ID"]),
                                Name = SafeString(dr["Name"]),
                                Description = SafeString(dr["Description"]),
                                ItemGroupID = SafeString(dr["ItemGroupID"]),
                                DefaultLocationID = SafeString(dr["DefaultLocationID"]),
                                ItemURL = SafeString(dr["ItemURL"]),
                                Color = SafeString(dr["Color"]),
                                Style = SafeString(dr["Style"]),
                                Material = SafeString(dr["Material"]),
                                UPC = SafeString(dr["UPC"]),
                                TagCount = SafeInt(dr["TagCount"]),
                                UseForRFID = SafeBool(dr["UseForRFID"]),
                                RFIDItemReference = SafeString(dr["RFIDItemReference"])
                            };

                            // Key is the item ID
                            if (!dcItems.ContainsKey(item.ID))
                                dcItems.Add(item.ID, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return dcItems;
        }
    }
}
