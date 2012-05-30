namespace Kendo.Mvc.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Script.Serialization;
    using System.Web.UI;
    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.Infrastructure;
    using Infrastructure.Implementation;
    using Kendo.Mvc.Resources;
    using Kendo.Mvc.UI.Fluent;
    using Kendo.Mvc.UI.Html;

    public class Grid<T> : ViewComponentBase, IGridColumnContainer<T>, IGrid where T : class
    {
        private readonly IGridHtmlBuilderFactory htmlBuilderFactory;

        private IGridUrlBuilder urlBuilder;

        private IGridDataKeyStore dataKeyStore;

        private string clientRowTemplate;

        public Grid(ViewContext viewContext, IJavaScriptInitializer initializer, IUrlGenerator urlGenerator,
            ILocalizationService localizationService, IGridHtmlBuilderFactory htmlBuilderFactory)
            : base(viewContext, initializer)
        {
            this.htmlBuilderFactory = htmlBuilderFactory;

            UrlGenerator = urlGenerator;

            PrefixUrlParameters = true;
            RowTemplate = new HtmlTemplate<T>();
            Columns = new List<GridColumnBase<T>>();
            DataKeys = new List<IDataKey>();

            Paging = new GridPagingSettings(this);
            Sorting = new GridSortSettings();
            Scrolling = new GridScrollingSettings();
            Navigatable = new GridNavigatableSettings(this);
            ColumnContextMenu = new GridColumnContextMenuSettings(this);
            Filtering = new GridFilteringSettings();

            Editing = new GridEditingSettings<T>(this)
            {                
                PopUp = new Window(viewContext, Initializer)
                {
                    Modal = true,
                    Draggable = true
                }                
            };

            Grouping = new GridGroupingSettings(this);
            Resizing = new GridResizingSettings();
            Reordering = new GridReorderingSettings();

            TableHtmlAttributes = new RouteValueDictionary();

            Footer = true;
            IsEmpty = true;

            Selection = new GridSelectionSettings();

            ToolBar = new GridToolBarSettings<T>(this);
            Localization = new GridLocalization(localizationService, CultureInfo.CurrentUICulture);
            NoRecordsTemplate = new HtmlTemplate();

            ValidationMetadata = new Dictionary<string, object>();

            AutoGenerateColumns = true;

            DataSource = new DataSource()
            {
                Type = DataSourceType.Server,
                ServerAggregates = true,
                ServerFiltering = true,
                ServerGrouping = true,
                ServerPaging = true,
                ServerSorting = true
            };

            DataSource.ModelType(typeof(T));
        }

        public DataSource DataSource
        {
            get;
            private set;
        }

        public IDictionary<string, object> ValidationMetadata
        {
            get;
            private set;
        }

        public IGridDetailTemplate<T> DetailTemplate
        {
            get;
            set;
        }

        public IDictionary<string, object> TableHtmlAttributes
        {
            get;
            private set;
        }

        public GridResizingSettings Resizing
        {
            get;
            private set;
        }

        public GridReorderingSettings Reordering
        {
            get;
            private set;
        }

        public bool Footer
        {
            get;
            set;
        }

        public GridLocalization Localization
        {
            get;
            set;
        }

        public GridToolBarSettings<T> ToolBar
        {
            get;
            private set;
        }

        public GridGroupingSettings Grouping
        {
            get;
            private set;
        }

        public GridEditingSettings<T> Editing
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the selection configuration
        /// </summary>
        public GridSelectionSettings Selection
        {
            get;
            private set;
        }

        internal IList<IDataKey> DataKeys
        {
            get;
            private set;
        }

        //TODO: Implement command button types
        private object Button<TButton>(T dataItem/*, GridButtonType buttonType*/, object htmlAttributes, object imageHtmlAttributes)
            where TButton : GridActionCommandBase, new()
        {
            var command = new TButton();
            
            //command.ButtonType = buttonType;
            command.ButtonType = GridButtonType.ImageAndText;

            //TODO: Implement command button html attributes
            //command.HtmlAttributes = htmlAttributes.ToDictionary();
            //command.ImageHtmlAttributes = imageHtmlAttributes.ToDictionary();

            var buttons = command.CreateDisplayButtons(Localization, UrlBuilder, new GridHtmlHelper<T>(ViewContext, DataKeyStore));

            var fragment = new HtmlFragment();

            buttons.Each(button => button.Create(dataItem).AppendTo(fragment));

            return MvcHtmlString.Create(fragment.ToString());
        }

        private object CustomButton<TButton>(
                   string name,
                   string text,
                    //TODO: Implement custom command routing
                   //string url,
                   //string actionName,
                   //string controllerName,
                   //string routeName,
                   //object routeValues,
                   //bool ajax,
                 //TODO: Implement command button types
                 //  GridButtonType buttonType,
                   object htmlAttributes,
                   object imageHtmlAttributes)

                   where TButton : GridCustomCommandBase, new()
        {
            var command = new TButton();

            //TODO: Implement command button types
            //command.ButtonType = buttonType;
            command.ButtonType = GridButtonType.ImageAndText;
            //TODO: Implement command button html attributes
            //command.HtmlAttributes = htmlAttributes.ToDictionary();
            //command.ImageHtmlAttributes = imageHtmlAttributes.ToDictionary();
            command.Text = text;
            //TODO: Implement custom command routing
            //command.Ajax = ajax;
            command.Name = name;

            //TODO: Implement custom command routing
            //if (url.HasValue())
            //{
            //    command.Url(url);
            //}

            //if (actionName.HasValue())
            //{
            //    command.Action(actionName, controllerName, routeValues);
            //    text = actionName;
            //}

            //if (routeName.HasValue())
            //{
            //    command.Route(routeName, routeValues);
            //    text = routeName;
            //}

            if (string.IsNullOrEmpty(command.Text))
            {
                command.Text = text;
            }

            var buttons = command.CreateDisplayButtons(Localization, UrlBuilder, new GridHtmlHelper<T>(ViewContext, DataKeyStore));
            var fragment = new HtmlFragment();
            buttons.Each(button => button.Create(null).AppendTo(fragment));

            return MvcHtmlString.Create(fragment.ToString());
        }

        private object CustomCommandToolBarButton(
            string name,
            string text,
            //TODO: Implement custom command routing
            //string url,
            //string actionName,
            //string controllerName,
            //string routeName,
            //object routeValues,
            //bool ajax,
          //TODO: Implement command button types
          //  GridButtonType buttonType,
            object htmlAttributes,
            object imageHtmlAttributes)
        {
            return CustomButton<GridToolBarCustomCommand<T>>(name, text/*, url, actionName, controllerName, routeName, routeValues, ajax, buttonType*/, htmlAttributes, imageHtmlAttributes);
        }

        //public object CustomCommandToolBarButton(
        //    string name,
        //    string text,
        //    //TODO: Implement custom command routing
        //    //string actionName,
        //    //string controllerName,
        //    //object routeValues,
        //    //bool ajax,
        //    //TODO: Implement command button types
        //    //GridButtonType buttonType,
        //    object htmlAttributes,
        //    object imageHtmlAttributes)
        //{
        //    return CustomCommandToolBarButton(name, text/*, null, actionName, controllerName, null, routeValues, ajax, buttonType*/, htmlAttributes, imageHtmlAttributes);
        //}

        //TODO: Implement custom command routing
        ////TODO: Implement command button types
        //public object CustomCommandToolBarButton(string name, string text, string actionName, string controllerName, bool ajax/*, GridButtonType buttonType*/, object htmlAttributes, object imageHtmlAttributes)
        //{
        //    return CustomCommandToolBarButton(name, text, actionName, controllerName, null, ajax/*, buttonType*/, htmlAttributes, imageHtmlAttributes);
        //}

        ////TODO: Implement command button types
        //public object CustomCommandToolBarButton(string name, string text, string actionName, string controllerName, object routeValues, bool ajax/*, GridButtonType buttonType*/)
        //{
        //    return CustomCommandToolBarButton(name, text, null, actionName, controllerName, null, routeValues, ajax/*, buttonType*/, null, null);
        //}

        //public object CustomCommandToolBarButton(string name, string text, string actionName, string controllerName, object routeValues)
        //{
        //    return CustomCommandToolBarButton(name, text, actionName, controllerName, routeValues, false);
        //}

        ////TODO: Implement command button types
        //public object CustomCommandToolBarButton(string name, string text, string routeName, RouteValueDictionary routeValues, bool ajax/*, GridButtonType buttonType*/, object htmlAttributes, object imageHtmlAttributes)
        //{
        //    return CustomCommandToolBarButton(name, text, null, null, null, routeName, routeValues, ajax/*, buttonType*/, htmlAttributes, imageHtmlAttributes);
        //}

        ////TODO: Implement command button types
        //public object CustomCommandToolBarButton(string name, string text, string routeName, RouteValueDictionary routeValues, bool ajax/*, GridButtonType buttonType*/)
        //{
        //    return CustomCommandToolBarButton(name, text, routeName, routeValues, ajax/*, buttonType*/, null, null);
        //}

        //public object CustomCommandToolBarButton(string name, string text, string routeName, RouteValueDictionary routeValues)
        //{
        //    return CustomCommandToolBarButton(name, text, routeName, routeValues, false/*, GridButtonType.ImageAndText*/, null, null);
        //}

        //TODO: Implement custom command routing
        //public object CustomCommandToolBarButton(string name, string text/*, string url, GridButtonType buttonType*/, object htmlAttributes, object imageHtmlAttributes)
        //{
        //    return CustomCommandToolBarButton(name, text, url/*, null, null, null, null, false, buttonType*/, htmlAttributes, imageHtmlAttributes);
        //}

        //TODO: Implement command button types
        //public object CustomCommandToolBarButton(string name, string text, string url, GridButtonType buttonType)
        //{
        //    return CustomCommandToolBarButton(name, text, url, buttonType, null, null);
        //}

        //public object CustomCommandToolBarButton(string name, string text, string url)
        //{
        //    return CustomCommandToolBarButton(name, text, url);
        //}

        public object CustomCommandToolBarButton(string name, string text)
        {
            return CustomCommandToolBarButton(name, text, null, null);
        }

        //TODO: Implement command button types
        public object EditButton(T dataItem/*, GridButtonType buttonType*/, object htmlAttributes, object imageHtmlAttributes)
        {
            Editing.Enabled = true;

            return Button<GridEditActionCommand>(dataItem/*, buttonType*/, htmlAttributes, imageHtmlAttributes);
        }

        //TODO: Implement command button types
        public object EditButton(T dataItem/*, GridButtonType buttonType*/, object htmlAttributes)
        {
            return EditButton(dataItem/*, buttonType*/, htmlAttributes, null);
        }

        //TODO: Implement command button types
        public object EditButton(T dataItem/*, GridButtonType buttonType*/)
        {
            return EditButton(dataItem/*, buttonType*/, null);
        }

        //TODO: Implement command button types
        //public object EditButton(T dataItem)
        //{
        //    return EditButton(dataItem, GridButtonType.ImageAndText);
        //}

        //TODO: Implement command button types
        public object DeleteButton(T dataItem/*, GridButtonType buttonType*/, object htmlAttributes, object imageHtmlAttributes)
        {
            Editing.Enabled = true;
            return Button<GridDeleteActionCommand>(dataItem/*, buttonType*/, htmlAttributes, imageHtmlAttributes);
        }

        public object DeleteButton(T dataItem/*, GridButtonType buttonType*/, object htmlAttributes)
        {
            return DeleteButton(dataItem/*, buttonType*/, htmlAttributes, null);
        }

        public object DeleteButton(T dataItem/*, GridButtonType buttonType*/)
        {
            return DeleteButton(dataItem/*, buttonType*/, null);
        }

        //TODO: Implement command button types
        //public object DeleteButton(T dataItem)
        //{
        //    return DeleteButton(dataItem, GridButtonType.ImageAndText);
        //}

        public object CreateButton(/*GridButtonType buttonType,*/ object htmlAttributes, object imageHtmlAttributes)
        {
            Editing.Enabled = true;
            InitializeEditors();
            return Button<GridToolBarCreateCommand<T>>(null/*, buttonType*/, htmlAttributes, imageHtmlAttributes);
        }

        public object CreateButton(/*GridButtonType buttonType,*/ object htmlAttributes)
        {
            return CreateButton(/*buttonType,*/ htmlAttributes, null);
        }

        public object CreateButton(/*GridButtonType buttonType*/)
        {
            return CreateButton(/*buttonType,*/ null);
        }

        //public object InsertButton()
        //{
        //    return InsertButton(GridButtonType.ImageAndText);
        //}

        public object SaveButton(/*GridButtonType buttonType,*/ object htmlAttributes, object imageHtmlAttributes)
        {
            Editing.Enabled = true;
            InitializeEditors();
            return Button<GridToolBarSaveCommand<T>>(null/*, buttonType*/, htmlAttributes, imageHtmlAttributes);
        }

        public object SaveButton(/*GridButtonType buttonType,*/ object htmlAttributes)
        {
            return SaveButton(/*buttonType,*/ htmlAttributes, null);
        }

        public object SaveButton(/*GridButtonType buttonType*/)
        {
            return SaveButton(/*buttonType,*/ null);
        }

        //public object SubmitChangesButton()
        //{
        //    return SubmitChangesButton(GridButtonType.ImageAndText);
        //}

        public string ClientRowTemplate
        {
            get
            {
                return clientRowTemplate;
            }
            set
            {
                clientRowTemplate = HttpUtility.HtmlDecode(value);
            }
        }

        IEnumerable<IDataKey> IGrid.DataKeys
        {
            get
            {
                return DataKeys.Cast<IDataKey>();
            }
        }

        /// <summary>
        /// Gets the template which the grid will use to render a row
        /// </summary>
        public HtmlTemplate<T> RowTemplate
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the filtering configuration.
        /// </summary>
        public GridFilteringSettings Filtering
        {
            get;
            private set;
        }        

        /// <summary>
        /// Gets the scrolling configuration.
        /// </summary>
        public GridScrollingSettings Scrolling
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the keyboard navigation configuration.
        /// </summary>
        public GridNavigatableSettings Navigatable
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the column context menu configuration.
        /// </summary>
        public GridColumnContextMenuSettings ColumnContextMenu
        {
            get;
            private set;
        }    

        public IUrlGenerator UrlGenerator
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether custom binding is enabled.
        /// </summary>
        /// <value><c>true</c> if custom binding is enabled; otherwise, <c>false</c>. The default value is <c>false</c></value>
        public bool EnableCustomBinding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the paging configuration.
        /// </summary>
        public GridPagingSettings Paging
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the columns of the grid.
        /// </summary>
        public IList<GridColumnBase<T>> Columns
        {
            get;
            private set;
        }

        IEnumerable<IGridColumn> IGrid.Columns
        {
            get
            {
                return Columns.Cast<IGridColumn>();
            }
        }

        public IList<GridColumnBase<T>> VisibleColumns
        {
            get
            {
                //TODO: Implement Column visibility
                return Columns/*.Where(c => c.Visible)*/.ToList();
            }
        }

        /// <summary>
        /// Gets the page size of the grid.
        /// </summary>
        public int PageSize
        {
            get
            {
                if (!Paging.Enabled)
                {
                    return 0;
                }

                return DataSource.PageSize;
            }
        }

        public int CurrentPage
        {
            get
            {
                return DataSource.Page;
            }
        }

        /// <summary>
        /// Gets the sorting configuration.
        /// </summary>
        /// <value>The sorting.</value>
        public GridSortSettings Sorting
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to add the <see cref="Name"/> property of the grid as a prefix in url parameters.
        /// </summary>
        /// <value><c>true</c> if prefixing is enabled; otherwise, <c>false</c>. The default value is <c>true</c></value>
        public bool PrefixUrlParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the action executed when rendering a row.
        /// </summary>
        public Action<GridRow<T>> RowAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the action executed when rendering a cell.
        /// </summary>
        public Action<GridCell<T>> CellAction
        {
            get;
            set;
        }

        public HtmlTemplate NoRecordsTemplate
        {
            get;
            private set;
        }

        public string Prefix(string parameter)
        {
            return PrefixUrlParameters ? Id + "-" + parameter : parameter;
        }

        public IEnumerable<AggregateDescriptor> Aggregates
        {
            get 
            {
                return DataSource.Aggregates;
            }
        }

        public override void WriteInitializationScript(TextWriter writer)
        {
            var options = new Dictionary<string, object>(Events);

            var autoBind = DataSource.Type != DataSourceType.Server;

            var columns = VisibleColumns.Select(c => c.ToJson());

            if (columns.Any())
            {
                options["columns"] = columns;
            }
            
            if (Grouping.Enabled)
            {
                options["groupable"] = true;
            }

            if (Paging.Enabled)
            {
                options["pageable"] = new Dictionary<string, object> { { "autoBind", autoBind } };
            }

            if (Sorting.Enabled)
            {
                var sorting = Sorting.ToJson();
                options["sortable"] = sorting.Any() ? (object)sorting : true;
            }

            if (Selection.Enabled)
            {
                options["selectable"] = String.Format("{0}, {1}", Selection.Mode, Selection.Type);
            }

            if (Filtering.Enabled)
            {
                var filtering = Filtering.ToJson();
                options["filterable"] = filtering.Any() ? (object)filtering : true;
            }

            if (Resizing.Enabled)
            {
                options["resizable"] = true;
            }

            if (Reordering.Enabled)
            {
                options["reorderable"] = true;
            }

            if (!Scrolling.Enabled)
            {
                options["scrollable"] = false;
            }

            if (Editing.Enabled && IsClientBinding)
            {
                options["editable"] = Editing.ToJson();
            }

            if (ToolBar.Enabled)
            {
                options["toolbar"] = ToolBar.ToJson();
            }

            if (autoBind == false)
            {
                options["autoBind"] = autoBind;
            }

            options["dataSource"] = DataSource.ToJson();

            if (HasDetailTemplate)
            { 
                options["detailTemplate"] = DetailTemplate.Serialize();
            }

            if (Navigatable.Enabled)
            {
                options["navigatable"] = true;
            }
            //TODO: Serialize editing
            //TODO: Localization
            //TODO: No records template

            writer.Write(Initializer.Initialize(Id, "Grid", options));

            base.WriteInitializationScript(writer);
        }

        internal int Colspan
        {
            get
            {
                int colspan = DataSource.Groups.Count + VisibleColumns.Count;

                if (DetailTemplate != null)
                {
                    colspan++;
                }

                return colspan;
            }
        }

        private string currentItemMode;

        private GridItemMode CurrentItemMode
        {
            get
            {
                if (currentItemMode == null)
                {
                    currentItemMode = this.ViewContext.Controller.ValueOf<string>(Prefix(GridUrlParameters.Mode));
                }

                return currentItemMode.ToEnum(GridItemMode.Default);
            }
        }

        /*TODO : Serialize data source
        public void SerializeDataSource(IClientSideObjectWriter writer)
        {
            IEnumerable dataSource = DataSource.Data;
            var dataTableEnumerable = dataSource as GridDataTableWrapper;

            var serverOperationMode = !DataSource.IsClientOperationMode;

            if (serverOperationMode)
            {
                dataSource = DataSource.Data;
            }

            if (dataTableEnumerable != null && dataTableEnumerable.Table != null)
            {
                dataSource = dataSource.SerializeToDictionary(dataTableEnumerable.Table);
            }
            else if (DataSource.Data is IQueryable<AggregateFunctionsGroup>)
            {
                var grouppedDataSource = DataSource.Data.Cast<IGroup>();

                if (serverOperationMode) {
                    dataSource = grouppedDataSource.Leaves();
                }
            }

            writer.AppendCollection("data", dataSource);
        }
        */

        protected override void WriteHtml(HtmlTextWriter writer)
        {
            if (!Columns.Any() && AutoGenerateColumns)
            {
                foreach (GridColumnBase<T> column in new GridColumnGenerator<T>(this).GetColumns())
                {
                    Columns.Add(column);
                }
            }

            var orignalClientValidationEnabled = ViewContext.ClientValidationEnabled;
            var originalFormContext = ViewContext.FormContext;

            try
            {
                ViewContext.ClientValidationEnabled = true;
                ViewContext.FormContext = new FormContext
                {
                    FormId = Name + "form"
                };

                if (Editing.Enabled && IsClientBinding)
                {
                    InitializeEditors();
                }
                
                AdjustColumnsTypesFromDynamic();

                if (!HtmlAttributes.ContainsKey("id"))
                {
                    HtmlAttributes["id"] = Id;
                }

                var builder = htmlBuilderFactory.CreateBuilder(Scrolling.Enabled);

                ProcessDataSource();

                var renderingData = CreateRenderingData();

                var functionalData = CreateFunctionalData();

                var container = builder.CreateGrid(HtmlAttributes, functionalData, renderingData);

                if (Editing.Mode == GridEditMode.PopUp && (CurrentItemMode == GridItemMode.Insert || CurrentItemMode == GridItemMode.Edit))
                {
                    AppendPopupEditor(container, renderingData);
                }

                container.WriteTo(writer);

                if (ViewContext.FormContext != null)
                {
                    ValidationMetadata.Add("Fields", ProcessValidationMetadata());

                    ValidationMetadata.Add("FormId", ViewContext.FormContext.FormId);
                }
            }
            finally
            {
                ViewContext.FormContext = originalFormContext;
                ViewContext.ClientValidationEnabled = orignalClientValidationEnabled;
            }

            base.WriteHtml(writer);
        }

        private GridFunctionalData CreateFunctionalData()
        {
            return new GridFunctionalData
            {
                ShowTopPager = Paging.Enabled && (Paging.Position == GridPagerPosition.Top || Paging.Position == GridPagerPosition.Both),
                ShowBottomPager = Paging.Enabled && (Paging.Position == GridPagerPosition.Bottom || Paging.Position == GridPagerPosition.Both),
                ShowTopToolBar = ToolBar.Enabled && (ToolBar.Position == GridToolBarPosition.Top || ToolBar.Position == GridToolBarPosition.Both),
                ShowBottomToolBar = ToolBar.Enabled && (ToolBar.Position == GridToolBarPosition.Bottom || ToolBar.Position == GridToolBarPosition.Both),
                ShowGroupHeader = Grouping.Enabled && Grouping.Visible,
                PagerData = CreatePagerData(),
                GroupingData = CreateGroupingData(),
                ToolBarData = CreateToolbarData(),
                ShowFooter = Footer
            };
        }
        
        private GridToolBarData CreateToolbarData()
        {
            return new GridToolBarData
            {
                Commands = ToolBar.Commands.Cast<IGridActionCommand>(),
                UrlBuilder = UrlBuilder,
                Localization = Localization,
                Template = ToolBar.Template
            };
        }

        private GridGroupingData CreateGroupingData()
        {
            return new GridGroupingData
            {
                GetTitle = VisibleColumns.Cast<IGridColumn>().GroupTitleForMember,
                GroupDescriptors = DataSource.Groups,
                Hint = Localization.GroupHint,
                UrlBuilder = UrlBuilder,
                SortedAscText = Localization.SortedAsc,
                SortedDescText = Localization.SortedDesc,
                UnGroupText = Localization.UnGroup
            };
        }

        private GridPagerData CreatePagerData()
        {
            return new GridPagerData
            {
                CurrentPage = DataSource.Page,
                PageCount = DataSource.TotalPages,
                Style = Paging.Style,
                UrlBuilder = UrlBuilder,
                Total = DataSource.Total,
                PageOfText = Localization.PageOf,
                PageText = Localization.Page,
                Colspan = Colspan,
                DisplayingItemsText = Localization.DisplayingItems,
                PageSize = DataSource.PageSize,
                RefreshText = Localization.Refresh
            };
        }

        private void AppendPopupEditor(IHtmlNode container, GridRenderingData renderingData)
        {
            var popup = Editing.PopUp;
            var cancelUrl = renderingData.UrlBuilder.CancelUrl(null);

            new WindowBuilder(popup)
                .Content(renderingData.PopUpContainer.InnerHtml)                
                //TODO: Add positioning of the window
                //.HtmlAttributes(new { style = "top:10%;left:50%;margin-left: -" + (popup.Width == 0 ? 360 : popup.Width) / 4 + "px" })                
                .Actions(buttons => buttons
                    .Close()
                );

            if (!IsClientBinding)
            {
                popup.Events["close"] = new ClientEvent { InlineCodeBlock = obj => "function(e) { e.preventDefault();" + string.Format("window.location.href = \"{0}\";", cancelUrl) + "}" };
            }
            
            if (!popup.Name.HasValue())
            {
                popup.Name = Name + "PopUp";
            }

            if (!popup.Title.HasValue())
            {
                popup.Title = CurrentItemMode == GridItemMode.Edit ? Localization.Edit : Localization.AddNew;
            }

            new LiteralNode(popup.ToHtmlString()).AppendTo(container);
        }

        private void ProcessDataSource()
        {
            if (Paging.Enabled && DataSource.PageSize == 0)
            {
                DataSource.PageSize = 10;
            }

            var binder = new DataSourceRequestModelBinder();

            if (this.PrefixUrlParameters)
            {
                binder.Prefix = Name;

                if (DataSource.Type == DataSourceType.Server)
                {
                    DataSource.Transport.Prefix = Name + "-";
                }
            }

            var controller = ViewContext.Controller;
            var bindingContext = new ModelBindingContext() { ValueProvider = controller.ValueProvider };

            var request = (DataSourceRequest)binder.BindModel(controller.ControllerContext, bindingContext);

            DataSource.Process(request, !EnableCustomBinding);

            if (DataSource.Schema.Model.Id != null)
            {
                DataKeys.Add(DataSource.Schema.Model.Id);
            }
        }

        private GridRenderingData CreateRenderingData()
        {
            var renderingData = new GridRenderingData
            {
                TableHtmlAttributes = TableHtmlAttributes,
                DataKeyStore = DataKeyStore,
                HtmlHelper = new GridHtmlHelper<T>(ViewContext, DataKeyStore),
                UrlBuilder = UrlBuilder,
                DataSource = DataSource.Data,
                Columns = VisibleColumns.Cast<IGridColumn>(),
                GroupMembers = DataSource.Groups.Select(g => g.Member),
                Mode = CurrentItemMode,
                EditMode = Editing.Mode,
                HasDetailTemplate = HasDetailTemplate,
                //TODO: Implement hidden columns
                Colspan = Colspan /*- Columns.Count(column => column.Hidden)*/,
                DetailTemplate = MapDetailTemplate(HasDetailTemplate ? DetailTemplate.Template : null),
                NoRecordsTemplate = FormatNoRecordsTemplate(),
                Localization = Localization,
                ScrollingHeight = Scrolling.Height,
                //EditFormHtmlAttributes = Editing.FormHtmlAttributes,
                ShowFooter = Footer && VisibleColumns.Any(c => c.FooterTemplate.HasValue() || c.ClientFooterTemplate.HasValue()),
                AggregateResults = DataSource.AggregateResults ?? new List<AggregateResult>(),
                Aggregates = Aggregates.SelectMany(aggregate => aggregate.Aggregates),
                GroupsCount = DataSource.Groups.Count,
                ShowGroupFooter = Aggregates.Any() && VisibleColumns.OfType<IGridBoundColumn>().Any(c => c.GroupFooterTemplate.HasValue()),
                PopUpContainer = new HtmlFragment(),
                CreateNewDataItem = () => Editing.DefaultDataItem(),
                //TODO: Implement insert row position
                //InsertRowPosition = Editing.InsertRowPosition,
                EditTemplateName = Editing.TemplateName,
                AdditionalViewData = Editing.AdditionalViewData,
                FormId = ViewContext.FormContext.FormId,
                Callback = RowActionCallback
            };

            if (RowTemplate.HasValue())
            {
                renderingData.RowTemplate = (dataItem, container) => RowTemplate.Apply((T)dataItem, container);
            }

            return renderingData;
        }

        private void RowActionCallback(GridItem item)
        {
            IsEmpty = false;

            if (RowAction != null)
            {
                var row = new GridRow<T>(this, (T)item.DataItem, item.Index);
                if (HasDetailTemplate)
                {
                    row.DetailRow = new GridDetailRow<T>
                    {
                        Html = item.DetailRowHtml
                    };
                }
                row.InEditMode = item.Type == GridItemType.EditRow;
                row.InInsertMode = item.Type == GridItemType.InsertRow;
                row.Selected = (item.State & GridItemStates.Selected) == GridItemStates.Selected;
                RowAction(row);

                if (HasDetailTemplate)
                {
                    item.Expanded = row.DetailRow.Expanded;
                    item.DetailRowHtml = row.DetailRow.Html;
                    item.DetailRowHtmlAttributes = row.DetailRow.HtmlAttributes;
                }

                if (row.Selected)
                {
                    item.State |= GridItemStates.Selected;
                }
                if (row.InEditMode)
                {
                    item.Type = GridItemType.EditRow;
                }
                else if (row.InInsertMode)
                {
                    item.Type = GridItemType.InsertRow;
                }
                else
                {
                    item.Type = GridItemType.DataRow;
                }

                item.HtmlAttributes = row.HtmlAttributes;
            }
        }

        private Action<object, IHtmlNode> MapDetailTemplate(HtmlTemplate<T> detailTemplate)
        {
            return (dataItem, container) =>
            {
                if (detailTemplate != null && detailTemplate.HasValue())
                    detailTemplate.Apply((T)dataItem, container);
            };
        }

        private HtmlTemplate FormatNoRecordsTemplate()
        {
            if (!NoRecordsTemplate.HasValue())
                NoRecordsTemplate.Html = Localization.NoRecords;

            return NoRecordsTemplate;
        }

        private IEnumerable<FieldValidationMetadata> ProcessValidationMetadata()
        {
            var validators = ViewContext.FormContext
                              .FieldValidators
                              .Values
                              .Where(IsBooleanField)
                              .ToArray();

            if (Name != null && Name.Contains("<#="))
            {
                validators = validators.Select((metadata) => EncodeRegularExpressionValidators(metadata)).ToArray();
            }
            
            return validators;
        }

        private static FieldValidationMetadata EncodeRegularExpressionValidators(FieldValidationMetadata metadata)
        {
            metadata.ValidationRules.Each(validationRule =>
            {
                if (validationRule.ValidationType == "regularExpression" || validationRule.ValidationType == "regex")
                {
                    if (validationRule.ValidationParameters.ContainsKey("pattern"))
                    {
                        validationRule.ValidationParameters["pattern"] =
                            new JavaScriptSerializer().Serialize(validationRule.ValidationParameters["pattern"]).Trim('"');
                    }
                }
            });
            return metadata;
        }

        private bool IsBooleanField(FieldValidationMetadata validationMetadata)
        {
            ModelMetadata modelMetadata = ModelMetadata.FromStringExpression(validationMetadata.FieldName, ViewContext.ViewData);

            return modelMetadata.ModelType != typeof(bool);
        }

        private void AdjustColumnsTypesFromDynamic()
        {
            if (!typeof (T).IsDynamicObject() || DataSource.Data == null ||
                !Columns.OfType<IGridBoundColumn>().Any(c => c.MemberType == null && c.Member.HasValue())
                ) 
                return;

            var processedDataSource = DataSource.Data;
            var firstItem = GetFirstItemFromGroups(processedDataSource);
            if (firstItem != null)
            {
                Columns.OfType<IGridBoundColumn>().Where(
                    c => c.MemberType == null && c.Member.HasValue()).Each(
                        c => c.MemberType = BindingHelper.ExtractMemberTypeFromObject(firstItem, c.Member));
            }
        }

        private static object GetFirstItemFromGroups(IEnumerable dataSource)
        {
            var groupItem = dataSource.OfType<IGroup>().FirstOrDefault();
            if (groupItem != null)
            {
                return groupItem.Leaves().Cast<object>().FirstOrDefault();
            }
            return dataSource.OfType<object>().FirstOrDefault();
        }

        internal bool OutputValidation
        {
            get
            {
                return (CurrentItemMode == GridItemMode.Insert || CurrentItemMode == GridItemMode.Edit || (Editing.Enabled && IsClientBinding))
                       && !ViewContext.UnobtrusiveJavaScriptEnabled
                    ;
            }
        }

        public bool AutoGenerateColumns { get; set; }

        public bool IsEmpty
        {
            get;
            set;
        }

        public bool IsClientBinding
        {
            get
            {
                return DataSource.Type == DataSourceType.Ajax;                
            }
        }
        
        public override void VerifySettings()
        {
            base.VerifySettings();
            
            this.ThrowIfClassIsPresent("k-grid-rtl", TextResource.Rtl);

            if (IsClientBinding)
            {
                if (Columns.OfType<IGridTemplateColumn<T>>().Where(c => c.Template != null && string.IsNullOrEmpty(c.ClientTemplate)).Any())
                {
                    throw new NotSupportedException(TextResource.CannotUseTemplatesInAjaxOrWebService);
                }

                if (DetailTemplate != null && DetailTemplate.Template.HasValue() && !DetailTemplate.ClientTemplate.HasValue())
                {
                    throw new NotSupportedException(TextResource.CannotUseTemplatesInAjaxOrWebService);
                }
            }

            if (!DataKeys.Any() && (Editing.Enabled || (Selection.Enabled && !IsClientBinding)))
            {
                throw new NotSupportedException(TextResource.DataKeysEmpty);
            }          

            if (Editing.Enabled)
            {
                if (HasCommandOfType<GridEditActionCommand>())
                {
                    if (!DataSource.Transport.Update.HasValue())
                    {
                        throw new NotSupportedException(TextResource.EditCommandRequiresUpdate);
                    }
                }

                if (HasCommandOfType<GridDeleteActionCommand>())
                {
                    if (!DataSource.Transport.Destroy.HasValue() && Editing.Mode != GridEditMode.InCell)
                    {
                        throw new NotSupportedException(TextResource.DeleteCommandRequiresDelete);
                    }
                }

                if (HasCommandOfType<GridToolBarCreateCommand<T>>())
                {
                    if (!DataSource.Transport.Create.HasValue() && Editing.Mode != GridEditMode.InCell)
                    {
                        throw new NotSupportedException(TextResource.InsertCommandRequiresInsert);
                    }
                }

                if (HasCommandOfType<GridToolBarSaveCommand<T>>())
                {
                    if (Editing.Mode != GridEditMode.InCell)
                    {
                        throw new NotSupportedException(TextResource.BatchUpdatesRequireInCellMode);
                    }

                    if (!DataSource.Transport.Update.HasValue())
                    {
                        throw new NotSupportedException(TextResource.BatchUpdatesRequireUpdate);
                    }
                }                

                if (Editing.Mode == GridEditMode.InCell) 
                {
                    if (!IsClientBinding)
                    {
                        throw new NotSupportedException(TextResource.InCellModeNotSupportedInServerBinding);
                    }

                    if (ClientRowTemplate.HasValue() || RowTemplate.HasValue())
                    {
                        throw new NotSupportedException(TextResource.InCellModeNotSupportedWithRowTemplate);
                    }
                }

                if(typeof(T) == typeof(System.Data.DataRowView) && Editing.Mode == GridEditMode.InLine 
                    && Columns.OfType<IGridBoundColumn>().Where(c => c.EditorTemplateName.HasValue()).Any())
                {
                    throw new NotSupportedException(TextResource.DataTableInLineEditingWithCustomEditorTemplates);
                }
            }
        }

        private bool HasCommandOfType<TCommand>()
        {
            return Columns.OfType<GridActionColumn<T>>().SelectMany(c => c.Commands).OfType<TCommand>().Any() ||
                ToolBar.Commands.OfType<TCommand>().Any();
        }        

        private void InitializeEditors()
        {            
            var skip = ViewContext.HttpContext.Items["$SelfInitialize$"] != null && (bool)ViewContext.HttpContext.Items["$SelfInitialize$"] == true;

            ViewContext.HttpContext.Items["$SelfInitialize$"] = true;            
            
            var dataItem = Editing.DefaultDataItem();

            var htmlHelper = new GridHtmlHelper<T>(ViewContext, DataKeyStore);

            if (Editing.Mode != GridEditMode.InLine && Editing.Mode != GridEditMode.InCell)
            {
                var container = new HtmlElement("div").AddClass(UIPrimitives.Grid.InFormContainer);

                htmlHelper.EditorForModel(dataItem, Editing.TemplateName, Columns.OfType<IGridForeignKeyColumn>().Select(c => c.SerializeSelectList), Editing.AdditionalViewData).AppendTo(container);

                EditorHtml = container.InnerHtml;
            }
            else
            {
                var cellBuilderFactory = new GridCellBuilderFactory();

                
                VisibleColumns.Each(column =>
                {
                    var cellBuilder = cellBuilderFactory.CreateEditCellBuilder(column, htmlHelper);
                    
                    var editor = cellBuilder.CreateCell(dataItem);

                    column.EditorHtml = editor.InnerHtml;
                });
            }

            if (!skip)
            {
                ViewContext.HttpContext.Items.Remove("$SelfInitialize$");
            }
        }
        public string EditorHtml { get; set; }

        public bool HasDetailTemplate
        {
            get
            {
                return DetailTemplate != null;
            }
        }

        private IGridDataKeyStore DataKeyStore
        {
            get
            {
                if (dataKeyStore == null)
                {
                    var dataKeys = DataKeys.Cast<IDataKey>();
                    var currentKeyValues = DataKeys.Select(key => key.GetCurrentValue(ViewContext.Controller.ValueProvider)).ToArray();

                    dataKeyStore = new GridDataKeyStore(dataKeys, currentKeyValues);
                }
                return dataKeyStore;
            }
        }

        public IGridUrlBuilder UrlBuilder
        {
            get
            {
                if (urlBuilder == null)
                {
                    urlBuilder = new GridUrlBuilder(this, DataKeyStore);
                }

                return urlBuilder;   
            }
        }
    }
}
