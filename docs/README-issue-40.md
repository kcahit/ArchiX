# Issue #40 - Grafik Template JSON Export

## Overview
This document provides information about the JSON export of Issue #40: "Grafik Template - Extension kararı ve template çalışması" (Graphic Template - Extension decision and template work).

## File Location
`docs/issue-40-export.json`

## File Details
- **Size**: 16KB
- **Lines**: 147
- **Format**: JSON with UTF-8 encoding (supports Turkish characters)
- **Structure**: Validated JSON format

## Content Structure

The JSON file contains the following sections:

### 1. Issue Metadata
- Issue number: 40
- Title: "Grafik Template - Extension kararı ve template çalışması"
- Status: Open
- Labels: template, reporting, pivot, map, filters
- Created date: 2025-12-23

### 2. Comments
All comments from the issue discussion, including:
- Author information
- Timestamps
- Full comment content

### 3. Technical Decisions
Comprehensive technical decisions made during the discussion:

#### Pivot Library
- **Chosen**: PivotTable.js
- **Reason**: MIT licensed, completely free, commercial use allowed, server-independent JS
- **License**: MIT

#### Map Library
- **Options**: 
  - Chart.js + chartjs-chart-geo
  - Leaflet.js (for more interactive maps)

#### Filter Architecture
- **Approach**: Shared filter engine
- **Description**: The existing filter + slicer infrastructure from grid-template-filtreli.cshtml will be used for all reports
- **Shared Components**: 
  - filteredData
  - columnFilters
  - textFilters
  - slicerSelections
  - activeSlicerColumns
  - createColumnCheckList
  - rebuildSlicers
  - updateSlicerItems
- **Extract to**: /js/archix.report.core.js

### 4. Template Types

#### Grid Template
- **Status**: Completed
- **File**: grid-template-filtreli.cshtml
- **Description**: Grid report template ready

#### Pivot Template
- **Status**: Planned
- **File**: pivot-template.cshtml
- **Description**: Pivot template to be created, will use shared slicer engine
- **Function**: buildPivotFrom(filteredData)

#### Map Template
- **Status**: Planned
- **File**: map-template.cshtml
- **Description**: Map template to be created, will aggregate by country
- **Function**: aggregateByCountry(filteredData)

#### Card Templates
- **Status**: Planned
- **File**: card-template.cshtml
- **Description**: KPI card templates to be created
- **Function**: recalculateCards(filteredData)
- **Metrics**: 
  - Total sales
  - Average amount
  - Count
  - Selected country count

### 5. Slicer Requirements
- **Power BI-like Experience**: Yes
- **Description**: Users should be able to filter comfortably with visual slicers like in Power BI
- **Approach**: Two-layered structure
  - **Layer 1**: Report-level slicer (shared slicer engine from grid-template-filtreli)
    - Dynamic slicer UI based on column type
    - Number operators: between, >, <
    - Text operators: contains, startsWith
  - **Layer 2**: View-level configuration (Pivot library - PivotTable.js)
    - Drag-and-drop for Row/Column/Value fields
    - Renderer options (heatmap, bar)
    - Cell-based simple filters

### 6. Data Structure
- **Single Dataset**: const data = [...]
- **Filtered Output**: filteredData
- **Filter States**:
  - Column filters: columnFilters
  - Text filters: textFilters
  - Slicer selections: slicerSelections
- **Column Type Detection**: Slicer type will be determined based on column type (different for amounts, dates, etc.)

### 7. Implementation Plan
1. Move existing grid filter + slicer JS to a shared JS module (archix.report.core.js)
2. Create new Razor templates (pivot, map, cards)
3. Reference the shared script in templates
4. Call view update functions after applyAllFilters
5. Add MSBuild steps to ArchiX.WebHost.csproj

### 8. User Workflow
1. User selects from shared slicer panel: Year, Company, Status, Country, etc.
2. Grid and pivot both update with the same filteredData
3. User drags fields in pivot to try different views

## Usage

To read and use this JSON export:

```javascript
// JavaScript
const issueData = require('./docs/issue-40-export.json');
console.log(issueData.issue.technical_decisions);
```

```csharp
// C#
using System.Text.Json;
var json = File.ReadAllText("docs/issue-40-export.json");
var issueData = JsonSerializer.Deserialize<dynamic>(json);
```

```python
# Python
import json
with open('docs/issue-40-export.json', 'r', encoding='utf-8') as f:
    issue_data = json.load(f)
    print(issue_data['issue']['technical_decisions'])
```

## Key Takeaways

1. **No additional slicer library needed**: The chosen products (Chart.js, PivotTable.js, Leaflet/Chart.js Geo) don't require an extra slicer engine
2. **Shared filter engine**: All views (grid, pivot, map, cards) will use filteredData as the common data source
3. **Power BI-like experience**: The combination of shared slicer engine and pivot library drag-and-drop will provide a Power BI-like user experience
4. **Implementation efficiency**: The existing slicer mechanism is already powerful enough to be reused across all report types

## Next Steps

1. Extract the shared filter/slicer code from grid-template-filtreli.cshtml to wwwroot/js/archix.report.core.js
2. Create a pivot template (Razor + JS) skeleton that includes the shared slicer JS and uses filteredData
3. Add MSBuild steps to copy the pivot template
4. Implement map and card templates using the same pattern

## References

- **Grid Template**: `/src/ArchiX.Library.Web/Templates/Modern/Pages/grid-template-filtreli.cshtml`
- **PivotTable.js**: https://pivottable.js.org/
- **Chart.js**: https://www.chartjs.org/
- **Leaflet.js**: https://leafletjs.com/
