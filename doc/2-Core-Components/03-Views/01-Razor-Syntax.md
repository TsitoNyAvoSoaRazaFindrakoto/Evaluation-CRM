# Razor Syntax in ASP.NET MVC

Razor is the view engine used in ASP.NET MVC applications.

## Basic Syntax

### Code Expressions
```cshtml
@Model.PropertyName
@DateTime.Now
@(2 + 2)
```

### Code Blocks
```cshtml
@{
    var message = "Hello World";
    var time = DateTime.Now;
}
```

### Control Structures
```cshtml
@if (Model.IsValid)
{
    <p>Data is valid</p>
}

@foreach (var item in Model.Items)
{
    <div>@item.Name</div>
}
```

## Model Binding

### Strongly-Typed Views
```cshtml
@model YourNamespace.Models.Customer
```

### Using ViewData and ViewBag
```cshtml
@ViewData["Title"]
@ViewBag.Message
```

## HTML Helpers
```cshtml
@Html.TextBoxFor(m => m.Name)
@Html.ActionLink("Link Text", "Action", "Controller")
```
