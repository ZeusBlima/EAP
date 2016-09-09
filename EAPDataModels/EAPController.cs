using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAPDataModels
{
    public partial class EAPController : dtsEAP
    {
        private Grafo grafo;
        private Queue<Operacao> OperacoesPend;
        private Stack<Operacao> OperacoesExec;
        private bool inTrans = false;
        public Grafo Grafo
        {
            get
            {
                return grafo;
            }
        }
        public EAPController(IContainer container = null) : base()
        {
            this.grafo = new Grafo();
            OperacoesPend = new Queue<Operacao>();
            OperacoesExec = new Stack<Operacao>();
            

            TB_PREDECESSORAS.RowDeleting += TB_PREDECESSORAS_RowDeleting;
            TB_PREDECESSORAS.TableNewRow += TB_PREDECESSORAS_TableNewRow;
            TB_PREDECESSORAS.RowChanging += TB_PREDECESSORAS_RowChanging;

            TB_ATIVIDADE.TableNewRow += TB_ATIVIDADE_TableNewRow;
            TB_ATIVIDADE.RowDeleting += TB_ATIVIDADE_RowDeleting;
            TB_ATIVIDADE.RowChanging += TB_ATIVIDADE_RowChanging;
        }

        public void Mapear()
        {
            foreach (TB_ATIVIDADERow atividade in TB_ATIVIDADE)
            {
                grafo.AddVertice(atividade.ATIV_CODIGO, atividade.IsATIV_DURACAONull() ? 0 : atividade.ATIV_DURACAO);
            }

            foreach (TB_PREDECESSORASRow predecessora in TB_PREDECESSORAS)
            {
                Grafo.Vertice u = grafo.FindVertice(predecessora.PRED_CODIGO);
                Grafo.Vertice v = grafo.FindVertice(predecessora.SUCE_CODIGO);

                grafo.AddAresta(u, v);
            }
        }

        public bool BeginTrans()
        {
            if (OperacoesPend.Count != 0 || OperacoesExec.Count != 0 || inTrans)
            {
                return false;
            }
            inTrans = true;
            return true;
        }
        public bool CommitTrans()
        {
            if (!inTrans)
            {
                return false;
            }
            while (OperacoesPend.Count > 0)
            {
                Operacao op = OperacoesPend.Dequeue();
                if (!ExecuteOperacao(op))
                {
                    RollbackTrans();
                    return false;
                }
            }
            OperacoesExec.Clear();
            inTrans = false;
            return true;
        }
        public bool RollbackTrans()
        {
            if (!inTrans)
            {
                return false;
            }
            while (OperacoesExec.Count > 0)
            {
                Operacao op = OperacoesExec.Pop();
                if (!RevertaOperacao(op))
                {
                    inTrans = false;
                    return false;
                }
            }
            inTrans = false;
            return true;
        }
        private bool GerenciarOperacao(Operacao op)
        {
            if (inTrans) return EnfileirarOperacao(op);
            else return ExecuteOperacao(op);
        }
        private bool EnfileirarOperacao(Operacao op)
        {
            OperacoesPend.Enqueue(op);
            return true;
        }
        private bool ExecuteOperacao(Operacao op)
        {
            OperacoesExec.Push(op);
            return true;
        }
        private bool RevertaOperacao(Operacao op)
        {
            return ExecuteOperacao(op.GetInversa());
        }
        private void TB_ATIVIDADE_TableNewRow(object sender, System.Data.DataTableNewRowEventArgs e)
        {
            //throw new NotImplementedException();
        }
        private void TB_ATIVIDADE_RowChanging(object sender, System.Data.DataRowChangeEventArgs e)
        {
            //throw new NotImplementedException();
        }
        private void TB_ATIVIDADE_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e)
        {
            //throw new NotImplementedException();
        }        
        private void TB_PREDECESSORAS_TableNewRow(object sender, System.Data.DataTableNewRowEventArgs e)
        {
            
        }
        private void TB_PREDECESSORAS_RowChanging(object sender, System.Data.DataRowChangeEventArgs e)
        {
            TB_PREDECESSORASRow predecessora = (TB_PREDECESSORASRow)e.Row;
            Grafo.Aresta a = grafo.FindAresta(predecessora.PRED_CODIGO, predecessora.SUCE_CODIGO);
            if (a == null)
            {
                Grafo.Vertice u = grafo.FindVertice(predecessora.PRED_CODIGO);
                Grafo.Vertice v = grafo.FindVertice(predecessora.SUCE_CODIGO);

                grafo.AddAresta(u, v);
            }
        }
        private void TB_PREDECESSORAS_RowDeleting(object sender, System.Data.DataRowChangeEventArgs e)
        {
            TB_PREDECESSORASRow predecessora = (TB_PREDECESSORASRow)e.Row;

            Grafo.Aresta a = grafo.FindAresta(predecessora.PRED_CODIGO, predecessora.SUCE_CODIGO);
            Operacao op = new Operacao(Operacao.TipoOperacao.Rem, a);
            GerenciarOperacao(op);
        }                
        public new System.Data.XmlReadMode ReadXml(string fileName)
        {
            System.Data.XmlReadMode result = base.ReadXml(fileName);

            Mapear();

            return result;
        }
        public class Operacao
        {
            TipoOperacao tipoOperacao;
            TipoDado tipoDado;
            Grafo.Aresta aresta;
            Grafo.Vertice vertice;

            public Grafo.Aresta Aresta
            {
                get
                {
                    return aresta;
                }
            }
            public Grafo.Vertice Vertice
            {
                get
                {
                    return vertice;
                }
            }

            public Operacao(TipoOperacao o, Grafo.Aresta a)
            {
                tipoOperacao = o;
                tipoDado = TipoDado.Aresta;
                aresta = a;
            }

            public Operacao(TipoOperacao o, Grafo.Vertice v)
            {
                tipoOperacao = o;
                tipoDado = TipoDado.Vertice;
                vertice = v;
            }

            private Operacao(Operacao op)
            {
                tipoOperacao = op.tipoOperacao;
                tipoDado = op.tipoDado;
                aresta = op.aresta;
                vertice = op.vertice;
            }

            public Operacao GetInversa()
            {
                Operacao inversa = new Operacao(this);
                inversa.tipoOperacao = OperacaoInversa(inversa.tipoOperacao);
                return inversa;
            }

            private TipoOperacao OperacaoInversa(TipoOperacao tipo)
            {
                switch (tipo)
                {
                    case Operacao.TipoOperacao.Add: return Operacao.TipoOperacao.Rem;
                    case Operacao.TipoOperacao.Rem: return Operacao.TipoOperacao.Add;
                    default: return Operacao.TipoOperacao.Rem;
                }
            }

            public enum TipoOperacao
            {
                Add, Rem
            }
            public enum TipoDado
            {
                Aresta, Vertice
            }
        }

    }
}
