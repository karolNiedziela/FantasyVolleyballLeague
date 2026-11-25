<#
.SYNOPSIS
    Minimizes saved PLS pages for the Scraper tests.

.DESCRIPTION
    Keeps only what the parsers read (tables, boxes, links with ids, data-* attributes, the statistics widget iframes)
    and removes the rest: scripts, styles, images, comments, the page header, navigation and footer, the games
    slider, banners, news articles, the cookie dialog, other iframes and the fan club box with private contacts.

    The repository is public: never commit a full page. The PLS terms of use (point 10) forbid publishing their
    pages, news texts are copyrighted and the boxes contain personal data.

    After minimizing, run the Scraper tests: a parser that needs something removed here fails its test.

.EXAMPLE
    pwsh tools/fixtures/Minimize-PlsFixtures.ps1 -Path tests/FantasyVolleyballLeague.Scraper.Tests/Fixtures/team_2_tour52.html

.EXAMPLE
    pwsh tools/fixtures/Minimize-PlsFixtures.ps1 -Path C:\temp\team_2_tour52.html -OutDir tests/FantasyVolleyballLeague.Scraper.Tests/Fixtures
#>
param(
    # Pages to minimize.
    [Parameter(Mandatory)][string[]]$Path,

    # Where to write the minimized pages; by default each page is overwritten in place.
    [string]$OutDir
)

$ErrorActionPreference = 'Stop'

# The same HtmlAgilityPack version as the Scraper, from the NuGet cache (restored by any build).
$packagesProps = Join-Path $PSScriptRoot '..\..\Directory.Packages.props'
$version = ([xml](Get-Content $packagesProps)).Project.ItemGroup.PackageVersion |
    Where-Object { $_.Include -eq 'HtmlAgilityPack' } |
    Select-Object -ExpandProperty Version
$packagesRoot = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $HOME '.nuget\packages' }
Add-Type -Path (Join-Path $packagesRoot "htmlagilitypack\$version\lib\netstandard2.0\HtmlAgilityPack.dll")

$removedNodes = @(
    '//script', '//style', '//link', '//meta', '//noscript', '//svg', '//img', '//picture', '//video', '//comment()',
    # Page chrome only: the schedule sections have their own <header> elements, which the parser reads.
    '//body/header', '//body/nav', '//body/footer',
    "//section[contains(concat(' ', normalize-space(@class), ' '), ' games-slider ')]",
    "//section[contains(concat(' ', normalize-space(@class), ' '), ' section-baner ')]",
    "//div[contains(@class, 'cokie-manager')]",
    # News; the club list of the teams page uses the same container with article.team-box.
    "//article[contains(concat(' ', normalize-space(@class), ' '), ' message-box ')]",
    "//iframe[not(contains(@class, 'widget-team-'))]",
    # Fan club contacts (private phone numbers): the box content first, then its title.
    "//h4[normalize-space() = 'KLUB KIBICA']/following-sibling::div[1]",
    "//h4[normalize-space() = 'KLUB KIBICA']"
)

if ($OutDir) {
    New-Item -ItemType Directory -Force $OutDir | Out-Null
}

foreach ($file in $Path) {
    $source = (Resolve-Path $file).Path
    $originalLength = (Get-Item $source).Length
    $document = [HtmlAgilityPack.HtmlDocument]::new()
    $document.Load($source, [System.Text.Encoding]::UTF8)

    foreach ($xpath in $removedNodes) {
        $nodes = $document.DocumentNode.SelectNodes($xpath)
        if ($nodes) {
            foreach ($node in @($nodes)) {
                $node.Remove()
            }
        }
    }

    $styledNodes = $document.DocumentNode.SelectNodes('//*[@style]')
    if ($styledNodes) {
        foreach ($node in $styledNodes) {
            $node.Attributes.Remove('style')
        }
    }

    $target = if ($OutDir) { Join-Path $OutDir (Split-Path $source -Leaf) } else { $source }
    $html = $document.DocumentNode.OuterHtml -replace '(\r?\n)(\s*\r?\n)+', "`n"
    [System.IO.File]::WriteAllText($target, $html, [System.Text.UTF8Encoding]::new($false))
    '{0}: {1:N0} -> {2:N0} bytes' -f (Split-Path $source -Leaf), $originalLength, (Get-Item $target).Length
}
