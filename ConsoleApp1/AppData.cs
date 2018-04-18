using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoParser
{
    class AppData
    {
        private String id;
        private String source;
        private String owner;
        private String repoName;
        private String authors;

        public String ID
        {
            set { id = value; }
            get { return id; }
        }

        public String Source
        {
            set { source = value; }
            get { return source; }
        }

        public String Owner
        {
            set { owner = value; }
            get { return owner; }
        }

        public String RepoName
        {
            set { repoName = value; }
            get { return repoName; }
        }

        public String Authors
        {
            set { authors = value; }
            get { return authors; }
        }
    }
}
