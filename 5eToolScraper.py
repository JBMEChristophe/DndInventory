# coding: utf-8

import csv
import argparse
import xml.etree.ElementTree as ET

csv_data_index = {'Name':0, 'Source':1, 'Rarity':2, 'Type':3, 'Attunement':4, 'Properties':5, 'Weight':6, 'Value':7, 'Description':8}

class Item:
    def __init__(self):
        self.Name = None
        self.Type = None
        self.Cost = None
        self.Weight = None
        self.Source = None
        self.Rarity = None
        self.Attunement = None
        self.Properties = None
        self.Description = None

    def __repr__(self):
        return str(self)
    def __str__(self):
        return "{0}, {1}, {2}, {3}, {4}".format(self.Name, self.Type, self.Cost, self.Weight, self.Source)

def indent(elem, level=0, hor='\t', ver='\n'):
    i = ver + level * hor
    if len(elem):
        if not elem.text or not elem.text.strip():
            elem.text = i + hor
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
        for elem in elem:
            indent(elem, level + 1, hor, ver)
        if not elem.tail or not elem.tail.strip():
            elem.tail = i
    else:
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = i
            
def create_xml(export_list):
    id = 0
    items = ET.Element("ArrayOfCatalogItemModel")
    for obj in export_list:
        item = ET.SubElement(items,"CatalogItemModel")
        ID = ET.SubElement(item,"ID")
        ID.text = "{0}_{1}".format(obj.Source, obj.Name.replace(" ","_"))
        Name = ET.SubElement(item,"Name")
        Name.text = obj.Name
        Type = ET.SubElement(item,"Type")
        for t in obj.Type:
            ItemType = ET.SubElement(Type,"ItemType")
            ItemType.text = t
        Rarity = ET.SubElement(item,"Rarity")
        Rarity.text = obj.Rarity
        Attunement = ET.SubElement(item,"Attunement")
        Attunement.text = obj.Attunement
        Properties = ET.SubElement(item,"Properties")
        Properties.text = obj.Properties
        Description = ET.SubElement(item,"Description")
        Description.text = obj.Description
        Cost = ET.SubElement(item,"Cost")
        Cost.text = obj.Cost
        Weight = ET.SubElement(item,"Weight")
        Weight.text = obj.Weight
        Source = ET.SubElement(item,"Source")
        Source.text = obj.Source
        IsStackable = ET.SubElement(item,"IsStackable")
        IsStackable.text = "false"
        CellSpanX = ET.SubElement(item,"CellSpanX")
        CellSpanX.text = "1"
        CellSpanY = ET.SubElement(item,"CellSpanY")
        CellSpanY.text = "1"
        id += 1
            
    indent(items)
    tree = ET.ElementTree(items)
    tree.write("Items.xml",encoding='utf-8', xml_declaration=True)

parser = argparse.ArgumentParser()
parser.add_argument('-i', '--item_path', default='Items.csv', help='Item input path')
args = parser.parse_args()

export_items = []
with open(args.item_path, 'r') as f:
    reader = csv.reader(f)
    next(reader)
    for row in reader:
        export_item = Item()
        index = 0
        for column in row:
            value = column
            if('â€”' in value or 'Ã—' in value):
                value = value.replace('â€”','')
                value = value.replace('Ã—','x')

            if index == csv_data_index['Name']:
                export_item.Name = value
            if index == csv_data_index['Type']:
                tmp = []                
                for t in value.split(', '):
                    firstDelPos=t.find("(") # get the position of (
                    secondDelPos=t.find(")") # get the position of )
                    typeItem = t.replace(t[firstDelPos:secondDelPos+1], "") # remove the string between two delimiters
                    typeItem = typeItem.replace('\'', '')
                    typeItem = ''.join(x for x in typeItem.title() if not x.isspace())
                    tmp.append(typeItem)
                export_item.Type = tmp
            if index == csv_data_index['Value']:
                export_item.Cost = value
            if index == csv_data_index['Weight']:
                export_item.Weight = value
            if index == csv_data_index['Source']:
                export_item.Source = value
            if index == csv_data_index['Rarity']:
                export_item.Rarity = value
            if index == csv_data_index['Attunement']:
                export_item.Attunement = value
            if index == csv_data_index['Properties']:
                export_item.Properties = value
            if index == csv_data_index['Description']:
                export_item.Description = value
            index += 1
        export_items.append(export_item)

create_xml(export_items)

