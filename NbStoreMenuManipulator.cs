using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.DDRMenu;
using NEvoWeb.Modules.NB_Store;

namespace FortyFingers.NbStoreMenuManipulator
{
    public class NbStoreMenuManipulator : INodeManipulator
    {
        String _catid = "";
        String _tabid = "";
        String _catguidkey = "";
        private PortalSettings _portalSettings;

        private MenuNode _nodeToReplace = null;

        public List<MenuNode> ManipulateNodes(List<MenuNode> nodes, PortalSettings portalSettings)
        {
            _portalSettings = portalSettings;
            var nameValueCollection = HttpUtility.ParseQueryString(HttpContext.Current.Request.Url.Query);
            _catid = nameValueCollection["catid"];
            _tabid = nameValueCollection["tabid"];
            _catguidkey = nameValueCollection["category"];

            // see if we need to merge into the current pages, by searching for marker page [cat]
            int idx = 0;
            int menuTabId = Null.NullInteger;
            menuTabId = SharedFunctions.GetStoreSettingInt(_portalSettings.PortalId, "FortyFingers.CategoriesTab",
                                                           SharedFunctions.GetCurrentCulture());

            
            MenuNode catNod = FindByTabId(nodes, menuTabId);

            if (catNod != null)
            {
                _nodeToReplace = catNod;

                List<MenuNode> chgNodes = null;

                // we might need a reference to the parent
                MenuNode parentNode = null;

                // find the collection of MenuNodes where catNod is in
                if (catNod.Depth == 0)
                {
                    parentNode = catNod.Parent;
                    chgNodes = nodes; // take the toplevel nodes
                }
                else
                {
                    parentNode = FindByTabId(nodes, catNod.Parent.TabId); // get a reference to the parent too
                    chgNodes = parentNode.Children; // take the siblings of the same parent
                }

                // find the index of the tab to replace within it's parent
                var ix = chgNodes.IndexOf(_nodeToReplace);

                //var tempNode = GetTempNode(catNod.Parent);
                //chgNodes.Add(tempNode);

                AddNodes(catNod, parentNode, ix);

                // remove the old item
                chgNodes.Remove(catNod);
            }
            else
            {
                // no marker for page insert so do nothing.
                // nodes = catNodeList;
            }

            return nodes;
        }

        private Dictionary<int, int> _catIDsToRedirectToTab = null;
        private Dictionary<int, int> CatIDsToRedirectToTab
        {
            get
            {
                if (_catIDsToRedirectToTab == null)
                {
                    _catIDsToRedirectToTab = new Dictionary<int, int>();

                    var setting = SharedFunctions.GetStoreSetting(_portalSettings.PortalId,
                                                                  "manipulatorcatidontabid.text", SharedFunctions.GetCurrentCulture());
                    // formaat: catid1>tabid1;catid2>tabid2
                    foreach (var pair in setting.Split(';'))
                    {
                        // a pair must contain ">"
                        if (pair.Contains(">"))
                        {
                            int catId = Null.NullInteger;
                            int tabId = Null.NullInteger;

                            int.TryParse(pair.Split('>')[0], out catId);
                            int.TryParse(pair.Split('>')[1], out tabId);

                            if (catId > 0 && tabId > 0)
                            {
                                _catIDsToRedirectToTab.Add(catId, tabId);
                            }
                        }
                    }
                }
                return _catIDsToRedirectToTab;
            }
        }

        private MenuNode FindByText(List<MenuNode> nodes, string text)
        {
            MenuNode retval = null;

            // try to find
            retval = nodes.FirstOrDefault(n => n.Text.ToLower() == text.ToLower());
            // if not found, try childnodes
            if (retval != null) return retval;

            foreach (var menuNode in nodes)
            {
                if (menuNode.HasChildren())
                {
                    retval = FindByText(menuNode.Children, text);
                    // stop searching if found
                    if (retval != null) break;
                }
            }

            return retval;
        }

        private MenuNode FindByTabId(List<MenuNode> nodes, int tabId)
        {
            MenuNode retval = null;

            // try to find
            retval = nodes.FirstOrDefault(n => n.TabId == tabId);
            // if not found, try childnodes
            if (retval != null) return retval;

            foreach (var menuNode in nodes)
            {
                if (menuNode.HasChildren())
                {
                    retval = FindByTabId(menuNode.Children, tabId);
                    // stop searching if found
                    if (retval != null) break;
                }
            }

            return retval;
        }

        private void AddNodes(MenuNode nodeToReplace, MenuNode parent, int insertAtParent)
        {
            var catCtrl = new CategoryController();

            var topLevelCats = catCtrl.GetCategories(PS.PortalId, CurrentCulture.ToString(), 0);

            foreach (NB_Store_CategoriesInfo topLevelCat in topLevelCats)
            {
                // add the nodes for the toplevel category
                var catNode = Cat2MenuNode(topLevelCat, parent, _nodeToReplace.Text, topLevelCat.SEOName, insertAtParent);

                // add the nodes for the category
                AddChildCategoryNodes(catCtrl, topLevelCat, catNode, topLevelCat.SEOName);
                //if(children.Any()) catNode.Children.AddRange(children);
            }
        }

        private void AddChildCategoryNodes(CategoryController catCtrl, NB_Store_CategoriesInfo category, MenuNode parent, string parentUrlPath)
        {
            //var retval = new List<MenuNode>();


            // recursively handle it's children
            var childCats = catCtrl.GetCategories(PS.PortalId, CurrentCulture.ToString(), category.CategoryID);
            foreach (NB_Store_CategoriesInfo childCat in childCats)
            {
                var newNode = Cat2MenuNode(childCat, parent, _nodeToReplace.Text, String.Format("{0}/{1}", parentUrlPath, childCat.SEOName));
                //if (newNode.TabId != parent.TabId)
                //{
                //    parent.Children.Add(newNode);
                //}

                //retval.AddRange(GetChildCategoryNodes(catCtrl, childCat, parent));
            }
            //return retval;
        }

        private MenuNode Cat2MenuNode(NB_Store_CategoriesInfo category, MenuNode parent, string urlFindValue, string urlReplaceValue, int parentInsertAt = -1)
        {
            var retval = new MenuNode();
            retval.Parent = parent;

            // make a unique id in the nodelist
            retval.TabId = category.CategoryID + 1000000;

            // add the node to the child collection of the parent
            if (parent != null && parent.TabId != retval.TabId)
            {
                if (parentInsertAt < 0)
                    parent.Children.Add(retval);
                else
                    parent.Children.Insert(parentInsertAt, retval);
            }

            retval.Text = category.CategoryName;
            retval.Title = category.SEOName;

            if (CatIDsToRedirectToTab.ContainsKey(category.CategoryID))
                retval.Url = Globals.NavigateURL(CatIDsToRedirectToTab[category.CategoryID], "", string.Format("catid={0}", category.CategoryID));
            else
                retval.Url = Globals.NavigateURL(_nodeToReplace.TabId, "", string.Format("catid={0}", category.CategoryID));

            retval.Url = retval.Url.Replace(urlFindValue, urlReplaceValue);

            retval.Enabled = true;
            retval.Selected = PS.ActiveTab.TabID == _nodeToReplace.TabId && _catid == category.CategoryID.ToString();
            retval.Breadcrumb = retval.Selected;

            retval.Separator = false;
            retval.LargeImage = "";
            retval.Icon = "";

            return retval;
        }

        private static PortalSettings PS
        {
            get { return PortalSettings.Current; }
        }

        private static CultureInfo CurrentCulture
        {
            get { return System.Threading.Thread.CurrentThread.CurrentCulture; }
        }
    }
}