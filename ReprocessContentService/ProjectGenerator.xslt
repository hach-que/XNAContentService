<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/project">
    <Project DefaultTargets="Build" ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
      <PropertyGroup>
        <Configuration Condition="'$(Configuration)' == ''">Default</Configuration>
        <Platform Condition="'$(Platform)' == ''">AnyCPU</Platform>
        <ItemType>GenericProject</ItemType>
        <ProductVersion>10.0.0</ProductVersion>
        <SchemaVersion>2.0</SchemaVersion>
        <ProjectGuid>{77A3495A-216F-4561-9BA4-AD75CF67A36C}</ProjectGuid>
      </PropertyGroup>
      <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Default|AnyCPU'">
        <OutputPath>.\bin\Default</OutputPath>
      </PropertyGroup>
      <Import Project="$(MSBuildBinPath)\Microsoft.CSharp.targets" />
      <Target Name="Build" DependsOnTargets="_CopyDeployFilesToOutputDirectory" />
      <ItemGroup>
        <xsl:for-each select="item">
          <None>
            <xsl:attribute name="Include">
              <xsl:value-of select="." />
            </xsl:attribute>
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
          </None>
        </xsl:for-each>
      </ItemGroup>
    </Project>
  </xsl:template>
</xsl:stylesheet>
