---
title: Overview
page_title: DateRangePicker Overview | Telerik UI for ASP.NET Core HtmlHelpers
description: "Learn the basics when working with the Kendo UI DateRangePicker HtmlHelper for ASP.NET Core (MVC 6 or ASP.NET Core MVC)."
previous_url: /aspnet-core/helpers/html-helpers/daterangepicker
slug: htmlhelpers_daterangepicker_aspnetcore
position: 1
---

# DateRangePicker HtmlHelper Overview

The DateRangePicker is a container for holding start and end date inputs.

The DateRangePicker HtmlHelper extension is a server-side wrapper for the [Kendo UI DateRangePicker](https://demos.telerik.com/kendo-ui/daterangepicker/index) widget. For more information on the DateRangePicker HtmlHelper for ASP.NET MVC, refer to the [UI for ASP.NET MVC documentation](http://docs.telerik.com/aspnet-mvc/helpers/daterangepicker/overview).

## Basic Configuration

The following example demonstrates the basic configuration for the DateRangePicker.

```
    @(Html.Kendo().DateRangePicker()
        .Name("daterangepicker") // The name of the DateRangePicker is mandatory. It specifies the "id" attribute of the widget.
        .Min(new DateTime(1900, 1, 1)) // Sets the min date of the DateRangePicker.
        .Max(new DateTime(2099, 12, 31)) // Sets the min date of the DateRangePicker.
        .Range(r => r.Start(DateTime.Now).End(DateTime.Now.AddDays(10))) // Sets the range of the DateRangePicker.
    )
```

## Events

You can subscribe to all DateRangePicker [events](http://docs.telerik.com/kendo-ui/api/javascript/ui/daterangepicker#events). For a complete example on basic DateRangePicker events, refer to the [demo on using the events of the DateRangePicker](https://demos.telerik.com/aspnet-core/daterangepicker/events).

The following example demonstrates how to subscribe to events by a handler name.

```
    @(Html.Kendo().DateRangePicker()
      .Name("daterangepicker")
      .Events(e => e
            .Open("daterangepicker_open")
            .Close("daterangepicker_close")
            .Change("daterangepicker_change")
      )
    )
    <script>
    function daterangepicker_open() {
        // Handle the open event.
    }

    function daterangepicker_close() {
        // Handle the close event.
    }

    function daterangepicker_change() {
        // Handle the change event.
    }
    </script>
```

## Referencing Existing Instances

To reference an existing Kendo UI DateRangePicker instance, use the [`jQuery.data()`](http://api.jquery.com/jQuery.data/) method. Once a reference has been established, use the [DateRangePicker API](http://docs.telerik.com/kendo-ui/api/javascript/ui/daterangepicker#methods) to control its behavior.

The following example demonstrates how to access an existing DateRangePicker instance.

        // Place this after your Kendo UI DateRangePicker for ASP.NET Core declaration.
        <script>
        $(function() {
        // The Name() of the DateRangePicker is used to get its client-side instance.
            var daterangepicker = $("#daterangepicker").data("kendoDateRangePicker");
        });
        </script>

## See Also

* [Basic Usage of the DateRangePicker HtmlHelper for ASP.NET Core (Demo)](https://demos.telerik.com/aspnet-core/daterangepicker/index)
* [Using the API of the DateRangePicker HtmlHelper for ASP.NET Core (Demo)](https://demos.telerik.com/aspnet-core/daterangepicker/api)
* [JavaScript API Reference of the DateRangePicker](https://docs.telerik.com/kendo-ui/api/javascript/ui/daterangepicker)
