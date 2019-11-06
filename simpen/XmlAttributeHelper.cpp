#include "XmlAttributeHelper.h"

#include <mdlxmltools.fdf>

USING_NAMESPACE_BENTLEY_XMLINSTANCEAPI_NATIVE;

namespace XmlAttributeHelper {
    

bool getNodeValue(MSWChar* value, int* pMaxChars, XmlNodeRef node) {

    XmlNodeListRef nodeListRef;
    mdlXMLDomNode_getChildNodes(&nodeListRef, node);

    int numChildren = mdlXMLDomNodeList_getNumChildren(nodeListRef);
    bool res = false;
    for (int i = 0; i < numChildren; ++i) {
        XmlNodeRef child = NULL;
        mdlXMLDomNodeList_getChild(&child, nodeListRef, i);

        long type;
        mdlXMLDomNode_getNodeType(&type, child);

        if (type == 3) {
            mdlXMLDomNode_getValue(value, pMaxChars, child);
            res = true;
        }
        mdlXMLDomNode_free(child);
    }
    return res;
}


bool findChildNode(XmlNodeRef* childP, XmlNodeRef node, const MSWChar* childName) {

    if (node == NULL)
        return false;

    bool res = false;

    if (mdlXMLDomNode_hasChildNodes(node)) {
        XmlNodeListRef nodeListRef;
        mdlXMLDomNode_getChildNodes(&nodeListRef, node);

        XmlNodeRef child;
        int numChildren = mdlXMLDomNodeList_getNumChildren(nodeListRef);

        for (int i = 0; i < numChildren; ++i) {
            mdlXMLDomNodeList_getChild(&child, nodeListRef, i);

            int numn = 50;
            MSWChar wNodeName[50];
            mdlXMLDomNode_getName(wNodeName, &numn, child);

            if (wcscmp(wNodeName, childName) == 0) {
                mdlXMLDomNode_cloneNode(childP, child, true);
                res = true;
            }
            mdlXMLDomNode_free(child);
            if (res)
                break;
        }
        mdlXMLDomNodeList_free(nodeListRef);
    }
    return res;
}


bool findNodeFromInstance(XmlNodeRef* node, Bentley::WString strInst, const MSWChar* nodeName)
{
    XmlDomRef pDomRef;
    XmlNodeRef pNodeRef;
    if (SUCCESS != mdlXMLDom_createFromText(&pDomRef, 0, strInst.GetMSWCharCP()))
        return false;

    mdlXMLDom_getRootNode(&pNodeRef, pDomRef);

    bool res = findChildNode(node, pNodeRef, nodeName);

    mdlXMLDom_free(pDomRef);
    return res;
}


//int testInstance(Bentley::WString strInst)
//{
//    char sValue[50000];
//    mdlCnv_convertUnicodeToMultibyte(strInst.GetMSWCharCP(), -1, sValue, 200);

//    mdlOutput_messageCenterW(MESSAGE_WARNING, strInst.GetMSWCharCP(), strInst.GetMSWCharCP(), true);

//    return 0;
//}

}