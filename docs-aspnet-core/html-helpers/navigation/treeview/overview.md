---
title: Overview
page_title: TreeView Overview | Telerik UI for ASP.NET Core HtmlHelpers
description: "Learn the basics when working with the Kendo UI TreeView HtmlHelper for ASP.NET Core (MVC 6 or ASP.NET Core MVC)."
previous_url: /aspnet-core/helpers/html-helpers/treeview
slug: htmlhelpers_treeview_aspnetcore
position: 1
---

# TreeView HtmlHelper Overview

The [TreeView](http://docs.telerik.com/kendo-ui/controls/navigation/treeview/overview) displays hierarchical data in a traditional tree structure.

It supports user interaction through mouse or touch events and provides the [checkboxes]({% slug htmlhelpers_treeview_checkboxes_aspnetcore %}) and [drag-and-drop]({% slug htmlhelpers_treeview_drag_drop_aspnetcore %}) features.

The TreeView HtmlHelper extension is a server-side wrapper for the [Kendo UI TreeView](http://demos.telerik.com/kendo-ui/treeview/index) widget. For more information on the HtmlHelper, refer to the article on the [TreeView HtmlHelper for ASP.NET MVC](http://docs.telerik.com/aspnet-mvc/helpers/treeview/overview).

## Initializing the TreeView

The following example demonstrates how to define the TreeView by using the TreeView HtmlHelper.

```Razor
@(Html.Kendo().TreeView()
    .Name("treeview")
    .DataTextField("Name")
    .DataSource(dataSource => dataSource
        .Read(read => read
            .Action("Read_TreeViewData", "TreeView")
        )
    )
)
```
```Controller
public static IList<HierarchicalViewModel> GetHierarchicalData()
{
    var result = new List<HierarchicalViewModel>()
    {
        new HierarchicalViewModel() { ID = 1, ParendID = null, HasChildren = true, Name = "Parent item" },
        new HierarchicalViewModel() { ID = 2, ParendID = 1, HasChildren = true, Name = "Parent item" },
        new HierarchicalViewModel() { ID = 3, ParendID = 1, HasChildren = false, Name = "Item" },
        new HierarchicalViewModel() { ID = 4, ParendID = 2, HasChildren = false, Name = "Item" },
        new HierarchicalViewModel() { ID = 5, ParendID = 2, HasChildren = false, Name = "Item" }
    };

    return result;
}

public IActionResult Read_TreeViewData(int? id)
{
    var result = GetHierarchicalData()
        .Where(x => id.HasValue ? x.ParendID == id : x.ParendID == null)
        .Select(item => new {
            id = item.ID,
            Name = item.Name,
            expanded = item.Expanded,
            imageUrl = item.ImageUrl,
            hasChildren = item.HasChildren
        });

    return Json(result);
}
```

## Basic Configuration

The following example demonstrates the basic configuration of the TreeView HtmlHelper and how to get the TreeView instance.

    @(Html.Kendo().TreeView()
        .Name("treeview")
        .Checkboxes(true)
        .DragAndDrop(true)
        .DataTextField("Name")
        .DataSource(dataSource => dataSource
            .Read(read => read
                .Action("Employees", "TreeView")
            )
        )
    )

    <script type="text/javascript">
        $(function () {
            // The Name() of the TreeView is used to get its client-side instance.
            var treeview = $("#treeview").data("kendoTreeView");
            console.log(treeview);
        });
    </script>

## Functionality and Features

* [Data binding]({% slug htmlhelpers_treeview_binding_aspnetcore %})
* [Items]({% slug htmlhelpers_treeview_items_aspnetcore %})
* [Dragging and dropping]({% slug htmlhelpers_treeview_drag_drop_aspnetcore %})
* [Checkboxes]({% slug htmlhelpers_treeview_checkboxes_aspnetcore %})

## Events

The following example demonstrates the available TreeView events and how an event handler could be implemented for each of them. For a complete example on basic TreeView events, refer to the [demo on using the events of the TreeView](https://demos.telerik.com/aspnet-core/treeview/events).

    @(Html.Kendo().TreeView()
        .Name("treeview")
        .DataTextField("Name")
        .Checkboxes(true)
        .DragAndDrop(true)
        .DataSource(dataSource => dataSource
            .Read(read => read
                .Action("Employees", "TreeView")
            )
        )
        .Events(events => events
            .Change("onChange")
            .Select("onSelect")
            .Check("onCheck")
            .Collapse("onCollapse")
            .Expand("onExpand")
            .DragStart("onDragStart")
            .Drag("onDrag")
            .DragEnd("onDragEnd")
            .Drop("onDrop")
        )
    )

    <script type="text/javascript">
        function onChange(e) {
            console.log('Selected node changed to:', e.sender.select());
        }

        function onSelect(e) {
            console.log('Selected node:', e.node);
        }

        function onCheck(e) {
            console.log('Checked node:', e.node);
        }

        function onCollapse(e) {
            console.log('Collapsed node:', e.node);
        }

        function onExpand(e) {
            console.log('Expanded node:', e.node);
        }

        function onDragStart(e) {
            console.log('Started dragging:', e.sourceNode);
        }

        function onDrag(e) {
            console.log("Dragging:", e.sourceNode);
        }

        function onDragEnd(e) {
            console.log("Finished dragging:", e.sourceNode);
        }

        function onDrop(e) {
            console.log("Dropped:", e.sourceNode);
        }
    </script>

## See Also

* [Basic Usage of the TreeView HtmlHelper for ASP.NET Core (Demo)](https://demos.telerik.com/aspnet-core/treeview/index)
* [Using the API of the TreeView HtmlHelper for ASP.NET Core (Demo)](https://demos.telerik.com/aspnet-core/treeview/api)
* [JavaScript API Reference of the TreeView](http://docs.telerik.com/kendo-ui/api/javascript/ui/treeview)
