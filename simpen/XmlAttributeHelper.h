#pragma once

#ifndef XML_ATTRIBUTE_HELPER_H
#define XML_ATTRIBUTE_HELPER_H

#include <IModel\xmlinstanceapi.h>
#include <IModel\xmlinstanceidcache.h>
#include <IModel\xmlinstanceschemamanager.h>
#include <mdlxmltools.h>
#include <WString.h>

namespace XmlAttributeHelper {
    
bool getNodeValue(MSWChar* value, int* pMaxChars, XmlNodeRef node);

bool findChildNode(
	XmlNodeRef* childP, XmlNodeRef node, const MSWChar* childName);

bool findNodeFromInstance(
	XmlNodeRef* node, Bentley::WString strInst, const MSWChar* nodeName);

}

#endif // !XML_ATTRIBUTE_HELPER_H