using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Helpers;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using EAPDataModels;
using EAPBusiness;

namespace EAPWinForms
{
    public partial class MainForm : XtraForm
    {

        string currentFilePath = null;
        bool temAlteracoes = false;
        public MainForm()
        {
            InitializeComponent();
            InitSkinGallery();
            EnableButtons();

        }
        void InitSkinGallery()
        {
            SkinHelper.InitSkinGallery(rgbiSkins, true);
        }

        void InitData()
        {
            temAlteracoes = false;
            EAPController = new EAPController();

            EAPController.TB_ATIVIDADE.RowChanged += DataChanged;
            EAPController.TB_ATIVIDADE.RowDeleted += DataChanged;
            EAPController.TB_ATIVIDADE.TableNewRow += DataChanged;


            EAPController.TB_PREDECESSORAS.RowChanged += DataChanged;
            EAPController.TB_PREDECESSORAS.RowDeleted += DataChanged;
            EAPController.TB_PREDECESSORAS.TableNewRow += DataChanged;

            gridControl.DataSource = EAPController;
            gridControl.DataMember = "TB_ATIVIDADE";
            //bar1.Visible = true;
            panelControl.Visible = true;
            panelControl.Enabled = true;
        }

        private void DataChanged(object sender, EventArgs e)
        {
            EnableButtons(true, true, true, true);
            temAlteracoes = true;
        }

        void EnableButtons(bool New = true, bool Open = true, bool Save = false, bool SaveAs = false)
        {
            iNew.Enabled = New;
            iOpen.Enabled = Open;
            iSave.Enabled = Save;
            iSaveAs.Enabled = SaveAs;
        }

        private void gridView_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (e.Column == colATIV_PREDECESSORAS)
            {
                EAPController.TB_ATIVIDADERow rowAtiv = (EAPController.TB_ATIVIDADERow)gridView.GetDataRow((sender as DevExpress.XtraGrid.Views.Base.ColumnView).GetRowHandle(e.ListSourceRowIndex));
                if (rowAtiv.RowState != DataRowState.Deleted && rowAtiv.RowState != DataRowState.Detached)
                {
                    if (e.IsGetData)
                    {
                        DataRow[] predecessoras = EAPController.TB_PREDECESSORAS.Select(String.Format("SUCE_CODIGO = {0}", rowAtiv.ATIV_CODIGO), "PRED_CODIGO ASC");

                        string strPredecessoras = "";

                        foreach (DataRow predecessora in predecessoras)
                        {
                            strPredecessoras += String.Format("; {0}", predecessora["PRED_CODIGO"]);
                        }

                        if (strPredecessoras.Length > 2) strPredecessoras = strPredecessoras.Substring(2);

                        e.Value = strPredecessoras;
                    }
                    else if (e.IsSetData)
                    {
                        string strPredecessoras = e.Value.ToString();
                        string[] splitPred = strPredecessoras.Split(';');
                        List<int> listPred = new List<int>();
                        foreach (string pred in splitPred)
                        {
                            if (pred.Trim() != "")
                            {
                                listPred.Add(Convert.ToInt32(pred.Trim()));
                            }
                        }
                        DataRow[] predecessoras = EAPController.TB_PREDECESSORAS.Select(String.Format("SUCE_CODIGO = {0}", rowAtiv.ATIV_CODIGO), "PRED_CODIGO ASC");

                        EAPController.BeginTrans();
                        foreach (DataRow predecessora in predecessoras)
                        {
                            int codPred = Convert.ToInt32(predecessora["PRED_CODIGO"]);
                            if (!listPred.Contains(codPred))
                            {
                                predecessora.Delete();
                            }
                        }

                        foreach (int codPred in listPred)
                        {
                            EAPController.TB_PREDECESSORASRow predecessora = EAPController.TB_PREDECESSORAS.FindBySUCE_CODIGOPRED_CODIGO(rowAtiv.ATIV_CODIGO, codPred);
                            if (predecessora == null)
                            {
                                predecessora = EAPController.TB_PREDECESSORAS.NewTB_PREDECESSORASRow();
                                predecessora.SUCE_CODIGO = rowAtiv.ATIV_CODIGO;
                                predecessora.PRED_CODIGO = codPred;

                                EAPController.TB_PREDECESSORAS.AddTB_PREDECESSORASRow(predecessora);
                            }
                        }
                        EAPController.CommitTrans();
                    }
                }
            }

        }
        private bool CheckData()
        {
            if (temAlteracoes)
            {
                DialogResult result = XtraMessageBox.Show("Existem alterações nessa EAP que ainda não foram salvas. Deseja salvar as alterações?", "Pergunta", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    iSave_ItemClick(null, null);
                    return true;
                }
                else if (result == DialogResult.No)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void iNew_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (CheckData())
            {
                InitData();
                EnableButtons(true, true, false, false);                
                currentFilePath = null;                
            }
        }        

        private void iOpen_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (CheckData())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    InitData();
                    EAPController.ReadXml(openFileDialog.FileName);                    
                    EnableButtons(true, true, false, true);
                    currentFilePath = openFileDialog.FileName;
                }
                
            }
        }

        private void iSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (currentFilePath == null) iSaveAs_ItemClick(sender, e);
            else Salvar(currentFilePath);            
        }

        private void iSaveAs_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Salvar(saveFileDialog.FileName);
            }
        }

        private bool Salvar(string path)
        {
            EAPController.WriteXml(path);
            temAlteracoes = false;
            currentFilePath = path;
            EnableButtons(true, true, false, true);
            return true;
        }

        private void gridView_InitNewRow(object sender, DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
        {
            DataRow atividade = gridView.GetDataRow(e.RowHandle);
            atividade["ATIV_CODIGO"] = atividade["ATIV_ID"] = Convert.ToInt32(EAPBusiness.EAPBusiness.IsNull(EAPController.TB_ATIVIDADE.Compute("MAX(ATIV_CODIGO)", ""), 0)) + 1;
            gridView.PostEditor();
            gridView.UpdateCurrentRow();
        }
    }

}