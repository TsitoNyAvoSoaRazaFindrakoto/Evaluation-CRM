# Advanced Razor Features

## Sections and Layouts

### Defining Sections
```cshtml
@section Scripts {
    <script src="~/js/custom.js"></script>
}

@section Styles {
    <link href="~/css/custom.css" rel="stylesheet" />
}
```

### RenderSection in Layout
```cshtml
@RenderSection("Scripts", required: false)
```

## Partial Views

### Rendering Partials
```cshtml
@await Html.PartialAsync("_PartialName", model)
@Html.Partial("_PartialName")
@{
    Html.RenderPartial("_PartialName");
}
```

## Dynamic Content

### Dynamic Attributes
```cshtml
@{
    var myClass = "highlight";
    var myTitle = "Tooltip";
}
<div class="@myClass" title="@myTitle">
    Dynamic content
</div>
```

### Templated Delegates
```cshtml
@{
    Func<dynamic, object> strongTemplate = 
        @<strong>@item</strong>;
}
@strongTemplate("Bold Text")
```

### HTML Encoding
```cshtml
@{
    var userInput = "<script>alert('xss')</script>";
}
@Html.Encode(userInput)
@userInput     @* Auto-encoded *@
@Html.Raw(trustedHtml)
```
