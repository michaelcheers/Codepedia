using Codepedia.DB;
using System.Collections.Generic;
using System.Linq;

namespace Codepedia
{
    public class FolderNode
    {
        public int FolderID;
        public int Rank;
        public bool IsExpanded, IsSelected;
        public int? EntryID;
        public string Name, Slug;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public List<FolderNode>? ChildNodes;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

        public IEnumerable<FolderNode> Descendants ()
        {
            return ChildNodes == null ? new[] { this } : ChildNodes.SelectMany(n => n.Descendants()).Prepend(this);
        }

        public static List<FolderNode> FetchTopNodes (CodepediaContext db, int? entryID = null)
        {
            List<FolderNode> topNodes = new();

            var dbObjs = (from parentChild in db.Hierachies
                          let entry = parentChild.ChildNavigation.Entry
                          let lastCommit = entry == null ? null : entry.EntryCommits.OrderByDescending(ec => ec.TimeCommited).First().Commit
                          select new
                          {
                              Parent = parentChild.Parent,
                              FolderID = parentChild.Child,
                              Rank = parentChild.Idx,
                              EntryID = entry == null ? null : (int?)entry.Id,
                              Name = parentChild.ChildNavigation.Title ?? (lastCommit == null ? null : lastCommit.Name),
                              Slug = lastCommit == null ? null : lastCommit.Slug
                          }).ToDictionary(n => n.FolderID);

            Dictionary<int, FolderNode> nodeObjs = new();

#nullable enable
            FolderNode? CreateNode(int nodeID)
            {
                return nodeObjs.TryGetOrAdd(nodeID, () =>
                {
                    if (!dbObjs.TryGetValue(nodeID, out var dbObj))
                        return null;

                    bool isSelected = entryID != null && dbObj.EntryID == entryID;

                    FolderNode node = new FolderNode
                    {
                        FolderID = dbObj.FolderID,
                        Name = dbObj.Name,
                        Slug = dbObj.Slug,
                        Rank = dbObj.Rank,
                        IsSelected = isSelected,
                        EntryID = dbObj.EntryID
                    };

                    FolderNode? parent = CreateNode(dbObj.Parent);

                    if (parent != null)
                    {
                        parent.ChildNodes ??= new List<FolderNode>();
                        parent.ChildNodes.Add(node);
                    }
                    else
                        topNodes.Add(node);

                    if (isSelected)
                    {
                        while (parent != null && dbObjs.TryGetValue(parent.FolderID, out var parentNode))
                        {
                            parent.IsExpanded = true;
                            parent = CreateNode(parentNode.Parent);
                        }
                    }

                    return node;
                });
            }

            foreach (int nodeID in dbObjs.Keys)
            {
                CreateNode(nodeID);
            }

            topNodes = topNodes.OrderBy(t => t.Rank).ToList();

            foreach (FolderNode node in nodeObjs.Values)
            {
                if (node.ChildNodes == null) continue;
                node.ChildNodes = node.ChildNodes.OrderBy(n => n.Rank).ToList();
            }

            return topNodes;

            //{
            //    int entryVisitingID = commitInfo.FolderID;
            //    while ((from parentChild in DB.ChildEntries
            //            where parentChild.Child == entryVisitingID
            //            select new
            //            {
            //                FolderID = parentChild.Parent,
            //                LastCommit = parentChild.ParentNavigation.EntryCommits.OrderByDescending(ec => ec.TimeCommited).First().Commit,
            //                Children = 
            //                    DB.ChildEntries.Where(child_parent => child_parent.Parent == parentChild.Parent).Select(sibling => new
            //                    {
            //                        FolderID = sibling.Child,
            //                        LastCommit = sibling.ChildNavigation.EntryCommits.OrderByDescending(ec => ec.TimeCommited).First().Commit
            //                    }).ToList()
            //            }).FirstOrDefault() is {} parent
            //    )
            //    {
            //        Folders.Add((parent.LastCommit, parent.Children.Select(commitInfo => (commitInfo.FolderID, commitInfo.LastCommit)).ToList()));
            //        entryVisitingID = parent.FolderID;
            //    }
            //}
        }
    }
}
#nullable disable
