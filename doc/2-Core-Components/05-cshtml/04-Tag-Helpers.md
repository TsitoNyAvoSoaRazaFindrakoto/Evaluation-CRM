# Tag Helpers in Razor

## Basic Tag Helpers

### Form Tag Helper
```cshtml
<form asp-controller="Account" asp-action="Login" method="post">
    <input asp-for="Username" />
    <span asp-validation-for="Username"></span>
</form>
```

### Input Tag Helpers
```cshtml
<input asp-for="Email" class="form-control" />
<textarea asp-for="Description"></textarea>
<select asp-for="CountryId" asp-items="@Model.Countries"></select>
```

### Link Tag Helpers
```cshtml
<a asp-controller="Home" asp-action="Index">Home</a>
<img asp-append-version="true" src="~/images/banner.jpg" />
```

### Environment Tag Helper
```cshtml
<environment include="Development">
    <link rel="stylesheet" href="~/css/site.css" />
</environment>
<environment exclude="Development">
    <link rel="stylesheet" href="~/css/site.min.css" />
</environment>
```

### Cache Tag Helper
```cshtml
<cache enabled="true" expires-after="TimeSpan.FromMinutes(10)">
    <div>Cached content here</div>
</cache>
```
