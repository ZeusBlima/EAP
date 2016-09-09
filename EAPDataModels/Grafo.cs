using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAPDataModels
{
    public class Grafo
    {
        List<Vertice> vertices;
        List<Aresta> arestas;
        bool orientado = true;
        public List<Vertice> Vertices
        {
            get
            {
                return vertices;
            }
        }

        public List<Aresta> Arestas
        {
            get
            {
                return arestas;
            }
        }

        public Grafo()
        {
            vertices = new List<Vertice>();
            arestas = new List<Aresta>();
        }

        public Vertice AddVertice(Int32 codigo, Int32 peso)
        {
            Vertice v = new Vertice(codigo, peso);
            vertices.Add(v);
            return v;
        }

        public Aresta AddAresta(Vertice origem, Vertice destino, Int32 peso = 1)
        {
            if (vertices.Contains(origem) && vertices.Contains(destino))
            {
                Aresta e = new Aresta(origem, destino, peso);
                origem.AddArestaAdj(e);
                arestas.Add(e);
                return e;
            }
            throw new Exception("Não é possivel criar arestas entre vértices que não pertençam ao grafo.");
        }

        public Vertice FindVertice(Int32 codigo)
        {
            foreach (Vertice v in vertices)
            {
                if (v.Codigo == codigo) return v;
            }
            return null;
        }

        public Aresta FindAresta(Int32 origem, Int32 destino)
        {
            Vertice u = FindVertice(origem);
            Vertice v = FindVertice(destino);

            foreach (Aresta a in arestas)
            {
                if (a.Origem == u && a.Destino == v) return a;
            }
            return null;
        }

        public override String ToString()
        {
            String result = "";
            foreach (Vertice u in vertices)
            {
                result += u.Codigo + " -> ";
                foreach (Aresta e in u.ArestasAdj)
                {
                    Vertice v = e.Destino;
                    result += v.Codigo + ", ";
                }
                result += "\n";
            }
            return result;
        }

        public class Vertice
        {
            Int32 codigo;
            Int32 peso;
            List<Aresta> arestasAdj;

            public int Codigo
            {
                get
                {
                    return codigo;
                }
            }

            public List<Aresta> ArestasAdj
            {
                get
                {
                    return arestasAdj;
                }
            }

            public int Peso
            {
                get
                {
                    return peso;
                }

                set
                {
                    peso = value;
                }
            }

            public Vertice(Int32 codigo, Int32 peso = 1)
            {
                this.codigo = codigo;
                this.peso = peso;
                this.arestasAdj = new List<Aresta>();
            }

            public void AddArestaAdj(Aresta e)
            {
                arestasAdj.Add(e);
            }
        }

        public class Aresta
        {
            Vertice origem;
            Vertice destino;
            Int32 peso;

            public Vertice Origem
            {
                get
                {
                    return origem;
                }
            }

            public Vertice Destino
            {
                get
                {
                    return destino;
                }
            }

            public int Peso
            {
                get
                {
                    return peso;
                }
            }

            public Aresta(Vertice origem, Vertice destino, Int32 peso = 1)
            {
                this.origem = origem;
                this.destino = destino;
                this.peso = peso;
            }
        }
    }
}
