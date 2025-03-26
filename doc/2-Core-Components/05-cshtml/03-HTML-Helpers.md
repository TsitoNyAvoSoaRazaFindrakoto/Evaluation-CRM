# HTML Helpers in Razor

## Form Helpers

### Form Creation
```cshtml
@using (Html.BeginForm("Action", "Controller", FormMethod.Post))
{
    @Html.AntiForgeryToken()
    <!-- form content -->
}
```

### Input Fields
```cshtml
@Html.TextBoxFor(m => m.Name, new { @class = "form-control" })
@Html.TextAreaFor(m => m.Description)
@Html.PasswordFor(m => m.Password)
@Html.CheckBoxFor(m => m.IsActive)
```

### Dropdowns
```cshtml
@Html.DropDownListFor(m => m.CategoryId, 
    new SelectList(Model.Categories, "Id", "Name"))
```

### Labels and Validation
```cshtml
@Html.LabelFor(m => m.Name)
@Html.ValidationMessageFor(m => m.Name)
@Html.ValidationSummary(true)
```

## Display Helpers

### Display and Editor Templates
```cshtml
@Html.DisplayFor(m => m.CreatedDate)
@Html.EditorFor(m => m.Email)
```

### Links and URLs
```cshtml
@Html.ActionLink("Click here", "Action", "Controller")
@Url.Action("Action", "Controller")
@Html.Raw(Html.Encode(someText))
```
