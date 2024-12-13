Hopefully it should be pretty self explanitory, but to create your own exporter:

Create a derived type of BaseExporterInfo and BaseExportContext

Then, register your ExporterInfo class in the static constructor of the ExporterRegistry class