using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResearchFrom14.Common.Components
{
    public class PathTree
    {
        public Dictionary<string, PathTree> tree;
        public string CommonName = "";
        public string pathSoFar = "";
        public bool isOpen = false;

        private PathTree()
        {
            tree = new Dictionary<string, PathTree>();
        }

        public static PathTree makeEmptyRoot()
        {
            return new PathTree();
        }

        public bool open()
        {
            isOpen = true;
            return isOpen;
        }
        public bool toggle()
        {
            isOpen = !isOpen;
            return isOpen;
        }

        public bool toggle(string path)
        {
            if (path.Equals(CommonName))
            {
                isOpen = !isOpen;
                return isOpen;
            }
            int pathEnd = path.IndexOf('/');
            if (pathEnd < 0 || pathEnd > path.Length)
            {
                if (tree.ContainsKey(path))
                {
                    return tree[path].toggle(path);
                }
            }
            string name = path.Substring(0, pathEnd);
            if (tree.ContainsKey(name))
            {
                return tree[name].toggle(path.Substring(pathEnd + 1, path.Length - (pathEnd+1)));
            }
            return false;
        }

        public static PathTree makePathTree(string previousPath, string path)
        {
            //first case, both empty strings, no path exists
            if ((previousPath == null || previousPath.Length == 0) && (path == null || path.Length == 0))
                return null;

            //second case, remaining path is the last name
            PathTree parent = new PathTree();
            int pathEnd = path.IndexOf('/');
            if(pathEnd < 0 || pathEnd > path.Length)
            {
                if (previousPath == null || previousPath.Length == 0)
                {
                    parent.pathSoFar = parent.CommonName = path;
                }
                else
                {
                    parent.pathSoFar = previousPath + '/' + path;
                    parent.CommonName = path;
                }
                return parent;
            }
            //third case, path still has children to spawn
            parent.CommonName = path.Substring(0, pathEnd);
            parent.pathSoFar = ((previousPath == null || previousPath.Length == 0) ? "" : (previousPath + '/')) + parent.CommonName;
            parent.addChildren(makePathTree(parent.pathSoFar, path.Substring(pathEnd+1, path.Length - (pathEnd+1))));
            return parent;
        }

        public void addChildren(PathTree ptree)
        {
            if (!tree.ContainsKey(ptree.CommonName))
            {
                tree[ptree.CommonName] = ptree;
                return;
            }
            foreach(PathTree children in ptree.tree.Values)
            {
                tree[ptree.CommonName].addChildren(children);
            }
        }
        public string getFullPath()
        {
            return pathSoFar;
        }

        public virtual bool SimpleEquals(object obj)
        {
            PathTree eq = obj as PathTree;
            return eq.getFullPath().Equals(this.getFullPath());
        }

        public override bool Equals(object obj)
        {
            PathTree eq = obj as PathTree;
            if (!eq.getFullPath().Equals(this.getFullPath()))
                return false;
            if (tree.Count != eq.tree.Count)
                return false;

            bool isEqual = true;
            foreach(String key in tree.Keys)
            {
                if (!eq.tree.ContainsKey(key))
                    return false;
                isEqual = isEqual && tree[key].Equals(eq.tree[key]);
            }
            return isEqual;
        }
    }
}
