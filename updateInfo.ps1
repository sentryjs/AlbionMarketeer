$xmlFile = "D:\Projects\albiondatajs\assets\download\updateInfo.xml"

$xml = [xml](Get-Content -Path $xmlFile)

$node = $xml.selectSingleNode('//item/version')
$node.InnerText = $args[0]

$hashFromFile = Get-FileHash -Path "D:\Projects\albiondatajs\assets\download\AlbionMarketeerSetup.exe" -Algorithm MD5

$node2 = $xml.selectSingleNode('//item/checksum')
$node2.InnerText = $hashFromFile.Hash

Get-Content -Path $xmlFile

$xml.Save($xmlFile)