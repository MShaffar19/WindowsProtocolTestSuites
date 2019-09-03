﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.FileAccessService.WSP.Adapter
{
    public class MessageBuilderColumnParameter
    {
        public Guid Guid;

        public uint PropertyId;

        public ushort ValueOffset;

        public ushort StatusOffset;

        public ushort LengthOffset;

        public StorageType StorageType;
    }

    public class MessageBuilderParameter
    {
        public string[] PropertySet_One_DBProperties;

        public string[] PropertySet_Two_DBProperties;

        public Guid Array_PropertySet_One_Guid;

        public string[] Array_PropertySet_One_DBProperties;

        public Guid Array_PropertySet_Two_Guid;

        public string[] Array_PropertySet_Two_DBProperties;

        public Guid Array_PropertySet_Three_Guid;

        public string[] Array_PropertySet_Three_DBProperties;

        public Guid Array_PropertySet_Four_Guid;

        public string[] Array_PropertySet_Four_DBProperties;

        public uint EachRowSize;

        public uint EType;

        public uint BufferSize;

        public uint LCID_VALUE;

        public uint ClientBase;

        public uint RowsToTransfer;

        public int NumberOfSetBindingsColumns;

        public MessageBuilderColumnParameter[] ColumnParameters;
    }

    /// <summary>
    /// Message Builder class provides methods to 
    /// build MS-WSP request messages
    /// </summary>
    public class MessageBuilder
    {
        #region Fields

        /// <summary>
        /// Specifies a field is Used
        /// </summary>
        byte FIELD_USED = 0x01;
        /// <summary>
        /// Specifies a field is NOT used
        /// </summary>
        byte FIELD_NOT_USED = 0x00;
        /// <summary>
        /// Specifies alignment of 4 bytes
        /// </summary>
        byte OFFSET_4 = 4;
        /// <summary>
        /// Specifies alignment of 8 bytes
        /// </summary>
        byte OFFSET_8 = 8;
        /// <summary>
        /// Represent a search scope
        /// </summary>
        string searchScope = string.Empty;
        /// <summary>
        /// Represent a query string
        /// </summary>
        string queryString = string.Empty;

        /// <summary>
        /// Specifies the property type is ID
        /// </summary>
        int PROPERTY_ID = 0x1;

        /// <summary>
        /// Length of each message header
        /// </summary>
        int HEADER_LENGTH = 16;

        /// <summary>
        /// Specifies comma separated properties
        /// </summary>
        char[] delimiter = new char[] { ',' };
        /// <summary>
        /// Number of external property set count
        /// </summary>
        uint EXTERNAL_PROPSET_COUNT = 4;

        /// <summary>
        /// Value to XOR with for Header checksum
        /// </summary>
        uint CHECKSUM_XOR_VALUE = 0x59533959;

        /// <summary>
        /// Value to OR with for Safe Array Type
        /// </summary>
        ushort SAFE_ARRAY_TYPE = 0x2000;

        /// <summary>
        /// Value to OR with for Vector Type
        /// </summary>
        ushort VECTOR_TYPE = 0x1000;

        /// <summary>
        /// Command time out
        /// </summary>
        uint COMMAND_TIME_OUT = 0x1E;
        /// <summary>
        /// Content Restriction operation
        /// </summary>
        uint RELATION_OPERATION = 0x4;

        /// <summary>
        /// Id of Property Restrictin Node
        /// </summary>
        uint PROPERTY_RESTRICTION_NODE_ID = 5;

        /// <summary>
        /// Id of Content Restriction Node
        /// </summary>
        uint CONTENT_RESTRICTION_NODE_ID = 4;

        /// <summary>
        /// Wieghtage of the Node
        /// </summary>
        uint NODE_WEIGHTAGE = 1000;

        /// <summary>
        /// Means Logical AND operation between Node Restriction
        /// </summary>
        uint LOGICAL_AND = 0x1;

        //newC
        /// <summary>
        /// static value for chapter of CPMGetRowsIn message
        /// </summary>
        public static uint chapter;

        public static uint rowWidth = 72;


        public MessageBuilderParameter parameter;

        /// <summary>
        /// Indicates if we use 64-bit or 32-bit when building requests.
        /// </summary>
        public bool Is64bit = true;

        #endregion

        /// <summary>
        /// constructor takes the ITestSite as parameter
        /// </summary>
        /// <param name="testSite">Site from where it needs 
        /// to read configurable data</param>
        public MessageBuilder(MessageBuilderParameter parameter)
        {
            this.parameter = parameter;
        }

        #region MS-WSP Request Messages
        /// <summary>
        /// Builds CPMConnectIn message 
        /// </summary>
        /// <param name="clientVersion">Version of the
        /// protocol client</param>
        /// <param name="isRemote">If the query is remote, 
        /// then 1 else 0</param>
        /// <param name="userName">User initiating the connection</param>
        /// <param name="machineName">Client Machine Name</param>
        /// <param name="serverMachineName">Server Machine Name</param>
        /// <param name="catalogName">Name of the Catalog 
        /// under operation</param>
        /// <param name="languageLocale">Language Locale</param>
        /// <returns>CPMConnectIn message</returns>
        public CPMConnectIn GetConnectInMessage(uint clientVersion, int isRemote,
            string userName, string machineName, string serverMachineName,
            string catalogName, string languageLocale)
        {
            var message = new CPMConnectIn();

            message._iClientVersion = clientVersion;

            message._fClientIsRemote = (UInt32)isRemote;

            message.MachineName = machineName;

            message.UserName = userName;

            message.cPropSets = 2;

            message.PropertySet1 = GetPropertySet1(catalogName);
            // PropertySet1 specifying MachineName
            message.PropertySet2 = GetPropertySet2(serverMachineName);

            message.cExtPropSet = 4;

            message.aPropertySets = new CDbPropSet[4];

            message.aPropertySets[0] = GetAPropertySet1(languageLocale); //Language locale
            message.aPropertySets[1] = GetAPropertySet2(); // FLAGs
            message.aPropertySets[2] = GetAPropertySet3(serverMachineName); // server
            message.aPropertySets[3] = GetAPropertySet4(catalogName); // Catalog

            message.Header = new WspMessageHeader
            {
                _msg = WspMessageHeader_msg_Values.CPMConnectIn,
            };

            return message;
        }



        /// <summary>
        /// Gets QueryStatusIn message BLOB for a given cursor
        /// </summary>
        /// <param name="cursor">Cursor of CPMQueryOut Message</param>
        /// <returns>QueryStatusIn Message BLOB</returns>
        public byte[] GetCPMQueryStatusIn(uint cursor)
        {
            // this message has just one field _cursor
            byte[] bytes = BitConverter.GetBytes(cursor);
            // Add Message Header
            return AddMessageHeader(MessageType.CPMGetQueryStatusIn, bytes);
        }

        /// <summary>
        /// Gets QueryStatusExIn Message BLOB for a given cursor and bookmark
        /// </summary>
        /// <param name="cursor">Cursor of CPMQueryOut Message</param>
        /// <param name="bookMarkHandle">Handle of the Bookmark</param>
        /// <returns>QueryStatusExIn BLOB</returns>
        public byte[] GetCPMQueryStatusExIn(uint cursor, uint bookMarkHandle)
        {
            int index = 0;
            byte[] bytes = new byte[Constant.SIZE_OF_UINT/*Cursor Handle */
                + Constant.SIZE_OF_UINT/* BookMark Handle */];
            Helper.CopyBytes(bytes, ref index,
                BitConverter.GetBytes(cursor));
            Helper.CopyBytes(bytes, ref index,
                BitConverter.GetBytes(bookMarkHandle));
            return AddMessageHeader(MessageType.CPMGetQueryStatusExIn, bytes);
        }

        /// <summary>
        /// Gets the CPMCiStateInOut Message BLOB
        /// </summary>
        /// <returns>CPMCiStateInOut BLOB</returns>
        public CPMCiStateInOut GetCPMCiStateInOut()
        {
            var message = new CPMCiStateInOut()
            {
                Header = new WspMessageHeader()
                {
                    _msg = WspMessageHeader_msg_Values.CPMCiStateInOut,
                },

                State = new CPMCiState
                {
                    cbStruct = 0x0000003C,
                },
            };

            return message;
        }

        /// <summary>
        /// Gets the CPMUpdateDocumentsIn message BOLB.
        /// </summary>
        /// <param name="flag">type of update</param>
        /// <param name="flagRootPath">Boolean value indicating
        /// if the RootPath field specifies a path on which 
        /// to perform the update.</param>
        /// <param name="rootPath">Name of the path to be updated</param>
        /// <returns>CPMUpdateDocumentsIn BLOB</returns>
        public byte[] GetCPMUpdateDocumentsIn(uint flag,
            uint flagRootPath, string rootPath)
        {
            int documentsInMessageLength = 2 * Constant.SIZE_OF_UINT
                + 2 * rootPath.Length/* unicode character */;
            byte[] mainBlob = new byte[documentsInMessageLength];
            //================ Converting values into Bytes ============
            int index = 0;

            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(flag));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(flagRootPath));
            Helper.CopyBytes(mainBlob, ref index,
                Encoding.Unicode.GetBytes(rootPath));
            return AddMessageHeader(MessageType.CPMUpdateDocumentsIn,
                mainBlob);
        }

        /// <summary>
        /// Gets RatioFinishedIn Message BLOB
        /// </summary>
        /// <param name="cursor">Handle identifying the query for
        /// which to request completion information</param>
        /// <param name="quick">Unused field</param>
        /// <returns>CPMRatioFinishedIn BLOB</returns>
        public byte[] GetCPMRatioFinishedIn(uint cursor, uint quick)
        {
            int ratioFinishedInMessageLength = 2 * Constant.SIZE_OF_UINT;
            byte[] mainBlob = new byte[ratioFinishedInMessageLength];
            //================ Converting value into Bytes ==================
            int index = 0;
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(cursor));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(quick));
            return AddMessageHeader(MessageType.CPMRatioFinishedIn, mainBlob);
        }

        /// <summary>
        /// Gets CPMFetchValueIn Message BLOB
        /// </summary>
        /// <param name="workID">Document ID</param>
        /// <param name="cbSoFar">Number of bytes 
        /// previously transferred</param>
        /// <param name="cbChunk">Maximum number of bytes that the sender 
        /// can accept in a CPMFetchValueOut message</param>
        /// <returns>CPMFetchValueIn BLOB</returns>
        public byte[] GetCPMFetchValueIn(uint workID,
            uint cbSoFar, uint cbChunk)
        {
            byte[] padding = null;
            int messageOffset = 0;
            messageOffset += 4 * Constant.SIZE_OF_UINT;
            TableColumn[] col = GetTableColumnFromConfig();
            byte[] propSpec = GetFullPropSec(WspConsts.System_ItemName._guidPropSet,
                PROPERTY_ID, (int)WspConsts.System_ItemName.PrSpec, ref messageOffset);
            if (messageOffset % OFFSET_4 != 0)
            {
                padding = new byte[OFFSET_4 - messageOffset % OFFSET_4];
                for (int i = 0; i < padding.Length; i++)
                {
                    padding[i] = 0;
                }
                messageOffset += padding.Length;
            }
            byte[] mainBlob = new byte[messageOffset];
            uint cbPropSpec = (uint)propSpec.Length;
            //================ Converting values into Bytes ================
            int index = 0;
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(workID));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(cbSoFar));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(cbPropSpec));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(cbChunk));
            Helper.CopyBytes(mainBlob, ref index, propSpec);
            if (padding != null)
            {
                //byte[] paddingValue = new byte[padding];
                Helper.CopyBytes(mainBlob, ref index, padding);
            }
            return AddMessageHeader(MessageType.CPMFetchValueIn, mainBlob);
        }

        /// <summary>
        /// Gets CPMCompareBmkIn Message Blob
        /// </summary>
        /// <param name="cursor">Handle from the 
        /// CPMCreateQueryOut message</param>
        /// <param name="chapt">Handle of the 
        /// chapter containing the bookmarks to compare</param>
        /// <param name="bmkFirst">Handle to the 
        /// first bookmark to compare</param>
        /// <param name="bmkSecond">Handle to the 
        /// second bookmark to compare</param>
        /// <returns>CPMCompareBmkIn BLOB</returns>
        public byte[] GetCPMCompareBmkIn(uint cursor,
            uint chapt, uint bmkFirst, uint bmkSecond)
        {
            int compareBmkInMessageLength = 4 * Constant.SIZE_OF_UINT;
            byte[] mainBlob = new byte[compareBmkInMessageLength];
            //================ Converting values into Bytes ====================
            int index = 0;
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(cursor));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(chapt));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(bmkFirst));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(bmkSecond));
            return AddMessageHeader
                (MessageType.CPMCompareBmkIn, mainBlob);
        }

        /// <summary>
        /// Gets CPMRestartPositionIn Message BLOB
        /// </summary>
        /// <param name="cursor">Handle obtained from 
        /// a CPMCreateQueryOut message</param>
        /// <param name="chapt">Handle of a chapter from
        /// which to retrieve rows</param>
        /// <returns>CPMRestartPositionIn BLOB</returns>
        public byte[] GetRestartPositionIn(uint cursor, uint chapt)
        {
            int restartPositionInMessageLength = 2 * Constant.SIZE_OF_UINT;
            byte[] mainBlob = new byte[restartPositionInMessageLength];
            //================ Converting values into Bytes ====================
            int index = 0;
            Helper.CopyBytes(mainBlob, ref index, BitConverter.GetBytes(cursor));
            Helper.CopyBytes(mainBlob, ref index, BitConverter.GetBytes(chapt));
            return AddMessageHeader(MessageType.CPMRestartPositionIn, mainBlob);
        }

        //Update by v-aliche for delta testing
        ///// <summary>
        ///// Gets CPMStopAsyncIn Message Blob
        ///// </summary>
        ///// <param name="cursor">Handle from the
        ///// CPMCreateQueryOut message</param>
        ///// <returns>CPMStopAsyncIn BLOB</returns>
        //public byte[] GetStopAsyncIn(uint cursor)
        //{
        //    byte[] mainBlob = new byte[Constant.SIZE_OF_UINT];
        //    //================ Converting values into Bytes ===================
        //    int index = 0;
        //    Helper.CopyBytes(mainBlob, ref index,
        //        BitConverter.GetBytes(cursor));
        //    return AddMessageHeader
        //        (MessageType.CPMStopAsynchIn, mainBlob);
        //}

        /// <summary>
        /// Gets CPMFreeCursorIn Message Blob
        /// </summary>
        /// <param name="cursor">Handle from the 
        /// CPMCreateQueryOut message</param>
        /// <returns>CPMFreeCursorIn BLOB</returns>
        public byte[] GetFreeCursorIn(uint cursor)
        {
            byte[] mainBlob = new byte[Constant.SIZE_OF_UINT];
            //================ Converting values into Bytes ==============
            int index = 0;
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(cursor));
            return AddMessageHeader
                (MessageType.CPMFreeCursorIn, mainBlob);
        }

        /// <summary>
        /// Gets CPMGetApproximatePositionIn Message Blob
        /// </summary>
        /// <param name="cursor">Handle from 
        /// CPMCreateQueryOut message</param>
        /// <param name="chapt">Handle to the chapter
        /// containing the bookmark</param>
        /// <param name="bmk">Handle to the bookmark for
        /// which to retrieve the approximate position</param>
        /// <returns></returns>
        public byte[] GetApproximatePositionIn
            (uint cursor, uint chapt, uint bmk)
        {
            int approximatePositionInLength = 3 * Constant.SIZE_OF_UINT;
            byte[] mainBlob = new byte[approximatePositionInLength];
            //========== Converting values into Bytes ============
            int index = 0;
            Helper.CopyBytes
                (mainBlob, ref index, BitConverter.GetBytes(cursor));
            Helper.CopyBytes
                (mainBlob, ref index, BitConverter.GetBytes(chapt));
            Helper.CopyBytes
                (mainBlob, ref index, BitConverter.GetBytes(bmk));
            return AddMessageHeader
                (MessageType.CPMGetApproximatePositionIn, mainBlob);
        }

        /// <summary>
        /// Builds CPMDisconnect message
        /// </summary>
        /// <returns>CPMDisconnect BLOB</returns>
        public byte[] GetDisconnectMessage()
        {
            byte[] messageHeader = null;
            int index = 0;
            //Total message size
            uint messageValue = (uint)MessageType.CPMDisconnect;
            uint messageStatus = 0;
            uint checksum = 0;
            uint reserveField = 0;
            messageHeader = new byte[4 * Constant.SIZE_OF_UINT];
            Helper.CopyBytes
                (messageHeader, ref index, BitConverter.GetBytes(messageValue));
            Helper.CopyBytes
                (messageHeader, ref index, BitConverter.GetBytes(messageStatus));
            Helper.CopyBytes
                (messageHeader, ref index, BitConverter.GetBytes(checksum));
            Helper.CopyBytes
                (messageHeader, ref index, BitConverter.GetBytes(reserveField));

            return messageHeader;
        }

        /// <summary>
        /// Build PMFindIndicesIn message
        /// </summary>
        /// <param name="cWids"></param>
        /// <param name="cDepthPrev"></param>
        /// <returns>CPMFindIndices BLOB</returns>
        public byte[] GetCPMFindIndices(uint cWids, uint cDepthPrev)
        {
            int index = 0;
            int messageOffset = 4 * Constant.SIZE_OF_UINT;
            byte[] pwids = new byte[cWids];

            byte[] prgiRowPrev = new byte[cDepthPrev];

            byte[] mainBlob = new byte[messageOffset];
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(cWids));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(cDepthPrev));
            Helper.CopyBytes(mainBlob, ref index, pwids);
            Helper.CopyBytes(mainBlob, ref index, prgiRowPrev);

            return AddMessageHeader(MessageType.CPMFindIndicesIn, mainBlob);
        }

        /// <summary>
        /// Build CPMGetRowsetNotifyIn message
        /// </summary>
        /// <returns>CPMGetRowsetNotify BLOB</returns>
        public byte[] GetCPMGetRowsetNotify()
        {
            byte[] mainBlob = new byte[Constant.SIZE_OF_UINT];

            return AddMessageHeader
                (MessageType.CPMGetRowsetNotifyIn, mainBlob);
        }

        /// <summary>
        /// Build CPMSetScopePrioritizationIn message
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="eventFrequency"></param>
        /// <returns>CPMSetScopePrioritization BLOB</returns>
        public byte[] GetCPMSetScopePrioritization(uint priority, uint eventFrequency)
        {
            byte[] mainBlob = new byte[2 * Constant.SIZE_OF_UINT];

            int index = 0;
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(priority));
            Helper.CopyBytes(mainBlob, ref index,
                BitConverter.GetBytes(eventFrequency));
            return AddMessageHeader
                (MessageType.CPMSetScopePrioritizationIn, mainBlob);
        }

        /// <summary>
        /// Builds the CPMGetNotify request message
        /// </summary>
        /// <returns>CPMGetNotify BLOB</returns>
        public byte[] GetCPMGetNotify()
        {
            byte[] messageHeader = null;
            int index = 0;
            //Total message size
            uint messageValue = (uint)MessageType.CPMGetNotify;
            uint messageStatus = 0;
            uint checksum = 0;
            uint reserveField = 0;
            messageHeader = new byte[4 * Constant.SIZE_OF_UINT];
            Helper.CopyBytes
                (messageHeader, ref index, BitConverter.GetBytes(messageValue));
            Helper.CopyBytes
                (messageHeader, ref index, BitConverter.GetBytes(messageStatus));
            Helper.CopyBytes
                (messageHeader, ref index, BitConverter.GetBytes(checksum));
            Helper.CopyBytes
                (messageHeader, ref index, BitConverter.GetBytes(reserveField));

            return messageHeader;
        }
        #endregion

        #region Structures for CPMConnectIn message
        /// <summary>
        /// Gets the first PropertySet1 ConnectIn message
        /// </summary>
        /// <param name="messageOffset">offset from
        /// the starting of the message</param>
        /// <param name="catalogName">Name of the catalog</param>
        /// <returns>PropertySet BLOB</returns>
        private CDbPropSet GetPropertySet1(string catalogName)
        {
            var propSet = new CDbPropSet();

            uint cProperties = 4;

            propSet.guidPropertySet = WspConsts.DBPROPSET_FSCIFRMWRK_EXT;

            propSet.cProperties = cProperties;

            propSet.aProps = new CDbProp[0];

            // --Get First PropSet with Guid Value DBPROPSET_FSCIFRMWRK_EXT -

            foreach (string property in parameter.PropertySet_One_DBProperties)
            {
                switch (property)
                {
                    case "2":
                        // Creating CDBProp with PropId 2 
                        // for GUID type FSCIFRMWRK
                        var value_FSCIFRMWRK_2 = GetBaseStorageVariant(vType_Values.VT_LPWSTR, new VT_LPWSTR(catalogName));
                        AppendDBProp(ref propSet, 2, value_FSCIFRMWRK_2);
                        break;
                    case "3":
                        // Creating CDBProp with PropId 3 
                        //for GUID type FCCIFRMWRK
                        var value_FCCIFRMWRK_3 = GetVector(vType_Values.VT_LPWSTR, new VT_LPWSTR[] { new VT_LPWSTR(null) });
                        AppendDBProp(ref propSet, 3, value_FCCIFRMWRK_3);
                        break;
                    case "4":
                        // Creating CDBProp with PropId 4 
                        // for GUID type FSCIFRMWRK
                        var value_FSCIFRMWRK_4 = GetVector(vType_Values.VT_I4, new Int32[] { 1 });
                        AppendDBProp(ref propSet, 4, value_FSCIFRMWRK_4);
                        break;

                    case "7":
                        // Creating CDBProp with PropId 7 
                        //for GUID type FSCIFRMWRK
                        var value_FSCIFRMWRK_7 = GetBaseStorageVariant(vType_Values.VT_I4, (Int32)0);
                        AppendDBProp(ref propSet, 7, value_FSCIFRMWRK_7);
                        break;
                    default:
                        break;
                }
            }

            return propSet;
        }
        /// <summary>
        /// Gets the first PropertySet2 ConnectIn message
        /// </summary>
        /// <param name="messageOffset">offset from the 
        /// starting of the message</param>
        /// <param name="machineName">Name of the 
        /// connecting client</param>
        /// <returns>PropertySet BLOB</returns>
        private CDbPropSet GetPropertySet2(string machineName)
        {
            var propSet = new CDbPropSet();

            uint cProperties = 1;

            propSet.guidPropertySet = WspConsts.DBPROPSET_CIFRMWRKCORE_EXT;

            propSet.cProperties = cProperties;

            propSet.aProps = new CDbProp[0];

            foreach (string property in parameter.PropertySet_Two_DBProperties)
            {
                switch (property)
                {
                    case "2":
                        // Creating CDBProp with PropId 2 
                        //for GUID type CIFRMWRKCORE
                        var value_CIFRMWRKCORE_2 = GetBaseStorageVariant(vType_Values.VT_BSTR, new VT_BSTR(machineName));
                        AppendDBProp(ref propSet, 2, value_CIFRMWRKCORE_2);
                        break;
                    default:
                        break;
                }
            }

            return propSet;
        }
        /// <summary>
        /// PropertySet1 for extPropSets array
        /// </summary>
        /// <param name="messageOffset">messageOffset</param>
        /// <param name="languageLocale">language locale 
        /// of the client</param>
        /// <returns>PropertySet BLOB</returns>
        private CDbPropSet GetAPropertySet1(string languageLocale)
        {
            var propSet = new CDbPropSet();

            uint cProperties = 6;

            var guid = parameter.Array_PropertySet_One_Guid;

            propSet.cProperties = cProperties;

            propSet.guidPropertySet = guid;

            propSet.aProps = new CDbProp[0];

            // Compile aPropertySet1 with Guid Value DBPROPSET_MSIDXS_ROWSETEXT
            foreach (string property in parameter.Array_PropertySet_One_DBProperties)
            {
                switch (property)
                {
                    case "2":
                        // Creating CDBProp with PropId 2 
                        //for GUID type ROWSETEXT
                        var value_ROWSETEXT_2 = GetBaseStorageVariant(vType_Values.VT_I4, (Int32)0);
                        AppendDBProp(ref propSet, 2, value_ROWSETEXT_2);
                        break;
                    case "3":
                        // Creating CDBProp with PropId 3 
                        //for GUID type ROWSETEXT
                        var value_ROWSETEXT_3 = GetBaseStorageVariant(vType_Values.VT_BSTR, new VT_BSTR(languageLocale));
                        AppendDBProp(ref propSet, 3, value_ROWSETEXT_3);
                        break;
                    case "4":

                        // Creating CDBProp with PropId 4 
                        //for GUID type ROWSETEXT
                        var value_ROWSETEXT_4 = GetBaseStorageVariant(vType_Values.VT_BSTR, new VT_BSTR(""));
                        AppendDBProp(ref propSet, 4, value_ROWSETEXT_4);
                        break;

                    case "5":
                        // Creating CDBProp with PropId 5 
                        //for GUID type ROWSETEXT
                        var value_ROWSETEXT_5 = GetBaseStorageVariant(vType_Values.VT_BSTR, new VT_BSTR(""));
                        AppendDBProp(ref propSet, 5, value_ROWSETEXT_5);
                        break;

                    case "6":

                        // Creating CDBProp with PropId 6 
                        // for GUID type ROWSETEXT
                        var value_ROWSETEXT_6 = GetBaseStorageVariant(vType_Values.VT_I4, (Int32)0);
                        AppendDBProp(ref propSet, 6, value_ROWSETEXT_6);
                        break;

                    case "7":

                        // Creating CDBProp with PropId 7 
                        //for GUID type ROWSETEXT
                        var value_ROWSETEXT_7 = GetBaseStorageVariant(vType_Values.VT_I4, (Int32)0);
                        AppendDBProp(ref propSet, 7, value_ROWSETEXT_7);
                        break;
                    default:
                        break;
                }
            }
            return propSet;
        }
        /// <summary>
        /// PropertySet2 for extPropSets array
        /// </summary>
        /// <param name="messageOffset">messageOffset</param>
        /// <returns>PropertySet BLOB</returns>
        private CDbPropSet GetAPropertySet2()
        {
            var propSet = new CDbPropSet();

            uint cProperties = 10;
            var guid = parameter.Array_PropertySet_Two_Guid;

            propSet.cProperties = cProperties;
            propSet.guidPropertySet = guid;
            propSet.aProps = new CDbProp[0];

            //---- Compile aPropertySet2 with Guid Value DBPROPSET_QUERYEXT
            foreach (string property in parameter.Array_PropertySet_Two_DBProperties)
            {
                switch (property)
                {
                    case "2":
                        // Creating CDBProp with PropId 2
                        // for GUID type QUERYEXT
                        var value_QUERYEXT_2 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 2, value_QUERYEXT_2);
                        break;
                    case "3":
                        // Creating CDBProp with PropId 3 
                        // for GUID type QUERYEXT
                        var value_QUERYEXT_3 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 3, value_QUERYEXT_3);
                        break;
                    case "4":

                        // Creating CDBProp with PropId 4 
                        //for GUID type QUERYEXT
                        var value_QUERYEXT_4 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 4, value_QUERYEXT_4);
                        break;
                    case "5":
                        // Creating CDBProp with PropId 5 
                        //for GUID type QUERYEXT
                        var value_QUERYEXT_5 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 5, value_QUERYEXT_5);
                        break;
                    case "6":
                        // Creating CDBProp with PropId 6
                        //for GUID type QUERYEXT
                        var value_QUERYEXT_6 = GetBaseStorageVariant(vType_Values.VT_BSTR, new VT_BSTR(""));
                        AppendDBProp(ref propSet, 6, value_QUERYEXT_6);
                        break;
                    case "8":
                        // Creating CDBProp with PropId 8 
                        //for GUID type QUERYEXT
                        var value_QUERYEXT_8 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 8, value_QUERYEXT_8);
                        break;
                    case "10":

                        // Creating CDBProp with PropId 10 
                        //for GUID type QUERYEXT
                        var value_QUERYEXT_10 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 10, value_QUERYEXT_10);
                        break;
                    case "12":

                        // Creating CDBProp with PropId 12 
                        //for GUID type QUERYEXT
                        var value_QUERYEXT_12 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 12, value_QUERYEXT_12);
                        break;
                    case "13":
                        // Creating CDBProp with PropId 13 
                        //for GUID type QUERYEXT
                        var value_QUERYEXT_13 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 13, value_QUERYEXT_13);
                        break;

                    case "14":

                        // Creating CDBProp with PropId 14 
                        //for GUID type QUERYEXT
                        var value_QUERYEXT_14 = GetBaseStorageVariant(vType_Values.VT_BOOL, (UInt16)0x0000);
                        AppendDBProp(ref propSet, 14, value_QUERYEXT_14);
                        break;
                    default:
                        break;
                }
            }
            return propSet;
        }
        /// <summary>
        /// Gets PropertySet3 of external PropertySets array
        /// </summary>
        /// <param name="messageOffset">message Offset</param>
        /// <param name="serverName">Name of the Server to connect</param>
        /// <returns>BLOB</returns>
        private CDbPropSet GetAPropertySet3(string serverName)
        {
            var propSet = new CDbPropSet();

            uint cProperties = 1;
            var guid = parameter.Array_PropertySet_Three_Guid;

            propSet.cProperties = cProperties;
            propSet.guidPropertySet = guid;
            propSet.aProps = new CDbProp[0];


            foreach (string property in parameter.Array_PropertySet_Three_DBProperties)
            {
                switch (property)
                {
                    case "2":
                        // Creating CDBProp with PropId 2
                        // for GUID type FSCIFRMWRK_EXT
                        var value_FSCIFRMWRK_EXT_2 = GetBaseStorageVariant(vType_Values.VT_BSTR, new VT_BSTR(serverName));
                        AppendDBProp(ref propSet, 2, value_FSCIFRMWRK_EXT_2);
                        break;
                    default:
                        break;
                }
            }

            return propSet;
        }
        /// <summary>
        /// Gets PropertySet4 of external PropertySets array
        /// </summary>
        /// <param name="messageOffset">message Offset</param>
        /// <param name="catalogName">Name of the Catalog to connect</param>
        /// <returns>BLOB</returns>
        private CDbPropSet GetAPropertySet4(string catalogName)
        {
            var propSet = new CDbPropSet();

            uint cProperties = 3;



            var guid = parameter.Array_PropertySet_Four_Guid;

            propSet.cProperties = cProperties;
            propSet.guidPropertySet = guid;
            propSet.aProps = new CDbProp[0];


            //Compile aPropertySet4 with Guid Value DBPROPSET_CIFRMWRKCORE_EXT
            foreach (string property in parameter.Array_PropertySet_Four_DBProperties)
            {
                switch (property)
                {
                    case "2":
                        // Creating CDBProp with PropId 2 
                        //for GUID type CIFRMWRKCORE_EXT
                        var value_CIFRMWRKCORE_EXT_2 = GetBaseStorageVariant(vType_Values.VT_BSTR, new VT_BSTR(catalogName));
                        AppendDBProp(ref propSet, 2, value_CIFRMWRKCORE_EXT_2);
                        break;
                    case "3":
                        // Creating CDBProp with PropId 3 
                        //for GUID type CIFRMWRKCORE_EXT
                        var safeArrayForStr = GetSafeArray<VT_BSTR>(vType_Values.VT_BSTR, new VT_BSTR[] { new VT_BSTR("") }, 0);
                        AppendDBProp(ref propSet, 3, safeArrayForStr);
                        break;

                    case "4":
                        // Creating CDBProp with PropId 4
                        // for GUID type 
                        var safeArrayForInt = GetSafeArray<Int32>(vType_Values.VT_I4, new Int32[] { 0 }, 0);
                        AppendDBProp(ref propSet, 4, safeArrayForInt);
                        break;
                    default:
                        break;
                }
            }

            return propSet;
        }

        /// <summary>
        /// Adds DB PropertySet
        /// </summary>
        /// <param name="propertySet">PropertySet BLOB</param>
        /// <param name="id">DbPropId</param>
        /// <param name="columnValue">BLOB of the Value field</param>
        /// <param name="messageOffSet">messageOffSet</param>
        private void AppendDBProp(ref CDbPropSet propertySet, uint id, CBaseStorageVariant value)
        {
            uint dbPropId = id;
            uint dbPropOption = 0;
            uint dbPropStatus = 0;

            var prop = new CDbProp();

            prop.DBPROPID = dbPropId;

            prop.DBPROPOPTIONS = dbPropOption;

            prop.DBPROPSTATUS = dbPropStatus;

            prop.colid = GetColumnId();

            prop.vValue = value;

            propertySet.aProps = propertySet.aProps.Append(prop).ToArray();
        }

        /// <summary>
        /// Get ColumnId for PropertySet
        /// </summary>
        /// <param name="messageOffset">messageOffset</param>
        /// <returns>BLOB ColumnID</returns>
        private CDbColId GetColumnId()
        {
            var result = new CDbColId(WspConsts.EmptyGuid, 0);
            return result;
        }
        #endregion

        #region Structures of CPMgetRowsIn message
        /// <summary>
        /// Gets FullPropertySpec for a given GUID, eKIND and propSec Id
        /// </summary>
        /// <param name="guid">PropSpec GUID</param>
        /// <param name="kind">EKind</param>
        /// <param name="propSec">PropSec Id</param>
        /// <param name="messageOffset">offset from the 
        /// beginning of the message</param>
        /// <returns>full property spec structure BLOB</returns>
        private byte[] GetFullPropSec
            (Guid guid, int kind, int propSec, ref int messageOffset)
        {
            int startingIndex = messageOffset;
            int index = 0;
            byte[] padding = null;
            if (messageOffset % OFFSET_8 != 0)
            {
                padding = new byte[OFFSET_8 - messageOffset % OFFSET_8];
                for (int i = 0; i < padding.Length; i++)
                {
                    padding[i] = 0;
                }
                messageOffset += padding.Length;
            }
            messageOffset += Constant.SIZE_OF_GUID;
            uint ulKind = (uint)kind;
            uint pRpSec = (uint)propSec;
            messageOffset += 2 * Constant.SIZE_OF_UINT;
            byte[] mainBlob = new byte[messageOffset - startingIndex];
            if (padding != null)
            {
                Helper.CopyBytes(mainBlob, ref index, padding);
            }
            Helper.CopyBytes
                (mainBlob, ref index, guid.ToByteArray());
            Helper.CopyBytes
                (mainBlob, ref index, BitConverter.GetBytes(ulKind));
            Helper.CopyBytes
                (mainBlob, ref index, BitConverter.GetBytes(pRpSec));
            return mainBlob;
        }
        /// <summary>
        /// Gets the Seek Description type for the RowsIn message
        /// </summary>
        /// <param name="eType">eType</param>
        /// <returns>returns object form of SeekDescription variable</returns>
        private object GetSeekDescription(eType_Values eType)
        {
            object result = null;

            switch (eType)
            {
                case eType_Values.eRowSeekNext:
                    {
                        result = new CRowSeekNext()
                        {
                            _cskip = 0x00000000,
                        };
                    }
                    break;

                case eType_Values.eRowSeekAt:
                    {
                        result = new CRowSeekAt()
                        {
                            _bmkOffset = 2,
                            _cskip = 2,
                            _hRegion = 0,
                        };
                    }
                    break;

                case eType_Values.eRowSeekAtRatio:
                    {
                        result = new CRowSeekAtRatio()
                        {
                            _ulNumerator = 1,
                            _ulDenominator = 2,
                            _hRegion = 0,
                        };
                    }
                    break;

                default:
                    throw new InvalidOperationException("Unsupported eType!");
            }

            return result;
        }

        /// <summary>
        /// Gets CRestrictionArray structure
        /// </summary>
        /// <param name="queryString">Search Query String</param>
        /// <param name="searchScope">Search Query Scope</param>
        /// <returns>CRestrictionArray structure BLOB</returns>
        public CRestrictionArray GetRestrictionArray(string queryString, string searchScope)
        {
            var result = new CRestrictionArray();

            result.count = 0x01;

            result.isPresent = 0x01;

            result.Restriction = GetQueryPathRestriction(queryString, searchScope);

            return result;
        }

        /// <summary>
        /// Gets Node restriction specific to the query 
        /// scope and the queryText
        /// </summary>
        /// <param name="messageOffset">Offset from the
        /// beginning from the message</param>
        /// <param name="queryString"></param>
        /// <param name="searchScope"></param>
        /// <returns>CPropertyRestrictionNode structure BLOB</returns>
        public CRestriction GetQueryPathRestriction(string queryString, string searchScope)
        {
            var result = new CRestriction();

            result._ulType = CRestriction_ulType_Values.RTAnd;

            result.Weight = NODE_WEIGHTAGE;

            var node = new CNodeRestriction();

            node._cNode = 2;

            node._paNode = new CRestriction[2];

            node._paNode[0] = GetPropertyRestriction(searchScope);

            node._paNode[1] = GetContentRestriction(queryString);

            result.Restriction = node;

            return result;

        }

        /// <summary>
        /// Gets CPropertyRestriction specific to the query path
        /// </summary>
        /// <param name="messageOffset">offset from 
        /// the beginning of the message</param>
        /// <param name="searchScope">Scope of the current search</param>
        /// <returns></returns>
        private CRestriction GetPropertyRestriction(string searchScope)
        {
            var result = new CRestriction();

            result._ulType = CRestriction_ulType_Values.RTProperty;

            result.Weight = NODE_WEIGHTAGE;

            var node = new CPropertyRestriction();

            node._relop = _relop_Values.PREQ;

            node._Property = WspConsts.System_Search_Scope;

            node._prval = GetBaseStorageVariant(vType_Values.VT_LPWSTR, new VT_LPWSTR(searchScope));

            node._lcid = parameter.LCID_VALUE;

            result.Restriction = node;

            return result;
        }

        /// <summary>
        /// Get rowset event structure
        /// </summary>
        /// <param name="ENABLEROWSETEVENTS"></param>
        /// <param name="messageOffset"></param>
        /// <returns></returns>
        private CRowsetProperties GetRowSetProperties(bool ENABLEROWSETEVENTS)
        {
            var result = new CRowsetProperties();

            if (ENABLEROWSETEVENTS)
            {
                result._uBooleanOptions = _uBooleanOptions_Values.eScrollable | _uBooleanOptions_Values.eEnableRowsetEvents;
                Constant.DBPROP_ENABLEROWSETEVENTS = true;
            }
            else
            {
                result._uBooleanOptions = _uBooleanOptions_Values.eScrollable;
                Constant.DBPROP_ENABLEROWSETEVENTS = false;
            }

            result._ulMaxOpenRows = 0x00000000;

            result._ulMemoryUsage = 0x00000000;

            result._cMaxResults = 0x00000000;

            result._cCmdTimeout = COMMAND_TIME_OUT;

            return result;
        }


        /// <summary>
        /// Gets the ContentRestrictionNode structure specific 
        /// to the query Text
        /// </summary>
        /// <param name="messageOffset">offset from the 
        /// begining of the message</param>
        /// <param name="queryString">Query String of the search</param>
        /// <returns>ContentRestriction structure Node</returns>
        private CRestriction GetContentRestriction(string queryString)
        {
            var result = new CRestriction();

            result._ulType = CRestriction_ulType_Values.RTContent;

            result.Weight = NODE_WEIGHTAGE;

            var node = new CContentRestriction();

            node._Property = WspConsts.System_Search_Contents;

            node.Cc = (UInt32)queryString.Length;

            node._pwcsPhrase = queryString;

            node.Lcid = parameter.LCID_VALUE;

            node._ulGenerateMethod = _ulGenerateMethod_Values.GENERATE_METHOD_EXACT;

            result.Restriction = node;

            return result;

        }
        #endregion

        #region Helper methods for Base Storage Type
        /// <summary>
        /// Gets a Vector form of a Base Storage Type
        /// </summary>
        /// <param name="type">StorageType (Vector Item type)</param>
        /// <param name="inputvalues">StorageType
        /// (Vector Item values)</param>
        /// <returns>Vector Base Storage Type BLOB</returns>
        private CBaseStorageVariant GetVector<T>(vType_Values type, T[] inputvalues) where T : struct
        {
            var result = new CBaseStorageVariant();
            result.vType = type | vType_Values.VT_VECTOR;
            result.vData1 = 0;
            result.vData2 = 0;
            switch (type)
            {
                case vType_Values.VT_I4:
                    {
                        var vector = new VT_VECTOR<Int32>();
                        vector.vVectorElements = (UInt32)inputvalues.Length;
                        vector.vVectorData = inputvalues.Cast<Int32>().ToArray();
                        result.vValue = vector;
                    }
                    break;
                case vType_Values.VT_LPWSTR:
                    {
                        var vector = new VT_VECTOR<VT_LPWSTR>();
                        vector.vVectorElements = (UInt32)inputvalues.Length;
                        vector.vVectorData = inputvalues.Cast<VT_LPWSTR>().ToArray();
                        result.vValue = vector;
                    }
                    break;

                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets Safe Array of given StorageType
        /// </summary>
        /// <param name="type">Type of item(s)</param>
        /// <param name="arrayValue">Value of item(s)</param>
        /// <param name="features">Safe Array Features</param>
        /// <returns>Safe Array Storage Type</returns>
        private CBaseStorageVariant GetSafeArray<T>(vType_Values type, T[] arrayValue, ushort features) where T : struct
        {
            var result = new CBaseStorageVariant();
            result.vType = type | vType_Values.VT_ARRAY;
            result.vData1 = 0;
            result.vData2 = 0;

            var array = new SAFEARRAY<T>();
            array.cDims = 1;
            array.fFeatures = features;

            var tempBuffer = new WspBuffer();
            if (arrayValue[0] is IWspStructure)
            {

                (arrayValue[0] as IWspStructure).ToBytes(tempBuffer);

            }
            else
            {
                tempBuffer.Add(arrayValue[0]);
            }

            array.cbElements = (UInt32)tempBuffer.WriteOffset;

            array.Rgsabound = new SAFEARRAYBOUND[] { new SAFEARRAYBOUND() { cElements = (UInt32)arrayValue.Length, lLbound = 0 } };
            array.vData = arrayValue;

            result.vValue = array;

            return result;
        }

        /// <summary>
        /// Gets the bound for a Safe Array Type
        /// </summary>
        /// <param name="numberOfElements">Number of 
        /// items in safe array</param>
        /// <param name="lowerLimit">lower index of the safe array</param>
        /// <returns>Bound BLOB</returns>
        private byte[] GetBound(uint numberOfElements, uint lowerLimit)
        {
            byte[] value = new byte[2 * Constant.SIZE_OF_UINT];
            int index = 0;
            Helper.CopyBytes
                (value, ref index, BitConverter.GetBytes(numberOfElements));
            Helper.CopyBytes
                (value, ref index, BitConverter.GetBytes(lowerLimit));
            return value;
        }

        /// <summary>
        /// Gets CBaseStorgeVariant structure of a given type/value
        /// </summary>
        /// <param name="type">Type of the storage value</param>
        /// <param name="isArray">true if it is an array of items</param>
        /// <param name="inputValue">input value, if isArray
        /// is true, pass values as array of objects</param>
        /// <returns>CBaseStorageVariant BLOB</returns>
        private CBaseStorageVariant GetBaseStorageVariant(vType_Values type, object inputValue)
        {
            var result = new CBaseStorageVariant();
            ushort vType = (ushort)type;
            byte vData1 = 0;
            byte vData2 = 0;

            result.vType = (vType_Values)vType;

            result.vData1 = vData1;

            result.vData2 = vData2;

            result.vValue = inputValue;

            return result;
        }
        #endregion

        #region Query Trio Messages (QueryIn, SetBindingsIn and RowsIn)

        /// <summary>
        /// Gets the CPMSetBindingsIn message
        /// </summary>
        /// <param name="queryCursor">Query associated Cursor</param>
        /// <param name="columns">Array of TableColumns to be Queried</param>
        /// <param name="isValidBinding">True if the binding is valid</param>
        /// <returns>CPMSetBindingsIn message.</returns>
        public CPMSetBindingsIn GetCPMSetBindingsIn(uint queryCursor, out TableColumn[] columns, bool isValidBinding)
        {
            uint cursor = queryCursor;
            uint rows = (uint)parameter.EachRowSize;
            // SIZE of ColumnCount and Columns combined to be assigned later.
            uint dummy = 0;// Dummy value
            Random r = new Random();
            columns = GetTableColumnFromConfig();

            if (!isValidBinding)
            {
                // decreasing the number of bytes to fail the Bindings
                rows -= (uint)r.Next((int)rows - 10, (int)rows);
            }

            uint columnCount = (uint)columns.Length;

            var message = new CPMSetBindingsIn()
            {
                Header = new WspMessageHeader
                {
                    _msg = WspMessageHeader_msg_Values.CPMSetBindingsIn,
                },

                _hCursor = cursor,

                _cbRow = rows,

                _dummy = dummy,

                cColumns = columnCount,

                aColumns = columns.Select(column => GetTableColumn(column)).ToArray(),
            };

            return message;
        }

        /// <summary>
        /// Reads the table Columns details from the configuration file
        /// </summary>
        /// <returns>array of Table Column</returns>
        private TableColumn[] GetTableColumnFromConfig()
        {
            int numberofTableColumns = parameter.NumberOfSetBindingsColumns;
            TableColumn[] columns = new TableColumn[numberofTableColumns];
            for (int i = 0; i < numberofTableColumns; i++)
            {
                columns[i] = new TableColumn();
                columns[i].Guid = parameter.ColumnParameters[i].Guid;
                columns[i].PropertyId = parameter.ColumnParameters[i].PropertyId;
                columns[i].ValueOffset = parameter.ColumnParameters[i].ValueOffset;
                columns[i].StatusOffset = parameter.ColumnParameters[i].StatusOffset;
                columns[i].LengthOffset = parameter.ColumnParameters[i].LengthOffset;
                columns[i].Type = parameter.ColumnParameters[i].StorageType;
            }
            return columns;
        }
        #region Get Table Column for CPMSetBindingsIn message



        #endregion

        /// <summary>
        /// Gets ColumnSet structure
        /// </summary>
        /// <param name="messageOffset">offset from the
        /// beginning of the message</param>
        /// <returns>ColumnSet structure BLOB</returns>
        public CColumnSet GetColumnSet(int numberOfColumns = 2)
        {
            var result = new CColumnSet();

            // Index of Properties to be queried
            uint[] indexes = new uint[numberOfColumns];

            for (uint i = 0; i < numberOfColumns; i++)
            {
                indexes[i] = i;
            }
            // Links to the 'pidMapper' field

            result.count = (UInt32)indexes.Length;

            result.indexes = indexes;

            return result;
        }

        /// <summary>
        /// Gets the PIDMapper Structure
        /// </summary>
        /// <param name="messageOffset">Offset from 
        /// the beginning of the message</param>
        /// <returns>Pid Mapper structure BLOB</returns>
        private CPidMapper GetPidMapper()
        {
            var result = new CPidMapper();

            result.aPropSpec = new CFullPropSpec[]
            {
                WspConsts.System_ItemName,
                WspConsts.System_ItemFolderNameDisplay,
                WspConsts.System_Search_Scope,
                WspConsts.System_Search_Contents,
            };

            result.count = (UInt32)result.aPropSpec.Length;

            return result;
        }

        public CPMGetRowsIn GetCPMRowsInMessage(uint cursor, uint rowsToTransfer, uint rowWidth, uint cbReadBuffer, uint fBwdFetch, uint eType, out uint reserved)
        {
            reserved = 256;

            var message = new CPMGetRowsIn()
            {
                Header = new WspMessageHeader()
                {
                    _msg = WspMessageHeader_msg_Values.CPMGetRowsIn,
                },

                _hCursor = cursor,

                _cRowsToTransfer = rowsToTransfer,

                _cbRowWidth = rowWidth,

                _cbReserved = reserved,

                _cbReadBuffer = cbReadBuffer,

                _ulClientBase = 0,

                _fBwdFetch = fBwdFetch,

                eType = (eType_Values)eType,

                _chapt = chapter,

                SeekDescription = GetSeekDescription((eType_Values)eType),
            };

            return message;
        }

        /// <summary>
        /// Gets TableColumn structure from given values
        /// </summary>
        /// <param name="column">TableColumn information</param>
        /// <returns>CTableColumn structure.</returns>
        private CTableColumn GetTableColumn(TableColumn column)
        {
            var result = new CTableColumn()
            {
                PropSpec = new CFullPropSpec(column.Guid, column.PropertyId),

                vType = (vType_Values)column.Type,

                AggregateType = CAggregSpec_type_Values.DBAGGTTYPE_BYNONE,

                ValueOffset = column.ValueOffset,

                ValueSize = GetSize(column.Type),

                StatusOffset = column.StatusOffset,

            };

            if (column.LengthOffset != 0)
            {
                result.LengthOffset = column.LengthOffset;
            }

            return result;
        }

        /// <summary>
        /// Gets the CreateQueryIn message
        /// </summary>
        /// <param name="path">A null terminated unicode
        /// string representing the scope of search</param>
        /// <param name="queryText">A NON null terminated unicode string representing the query string</param>
        /// <param name="ENABLEROWSETEVENTS">flag for ENABLEROWSETEVENTS</param>
        public CPMCreateQueryIn GetCPMCreateQueryIn(string path, string queryText, bool ENABLEROWSETEVENTS)
        {
            searchScope = path;

            queryString = queryText;

            var message = new CPMCreateQueryIn();

            message.ColumnSet = GetColumnSet();

            message.RestrictionArray = GetRestrictionArray(queryString, searchScope);

            message.SortSet = null;

            message.CCategorizationSet = null;

            message.RowSetProperties = GetRowSetProperties(ENABLEROWSETEVENTS);

            message.PidMapper = GetPidMapper();

            message.GroupArray = new CColumnGroupArray()
            {
                count = 0,
                aGroupArray = new CColumnGroup[0]
            };

            message.Lcid = parameter.LCID_VALUE;

            message.Header = new WspMessageHeader
            {
                _msg = WspMessageHeader_msg_Values.CPMCreateQueryIn,
            };

            return message;
        }
        #endregion

        /// <summary>
        /// Gets CPMGetScopeStatisticsIn message BLOB 
        /// </summary>
        /// <returns>CPMGetScopeStatisticsIn Message BLOB</returns>
        public byte[] GetCPMGetScopeStatisticsIn()
        {
            // Add Message Header
            byte[] mainBlob = new byte[Constant.SIZE_OF_UINT];
            return AddMessageHeader(MessageType.CPMGetScopeStatisticsIn, mainBlob);
        }

        #region Utility Methods
        /// <summary>
        /// Adds Header to a Message and also the checksum if required
        /// </summary>
        /// <param name="msgType">Type of message</param>
        /// <param name="messageBlob">Message BLOB</param>
        /// <returns>Message BLOB with Message Header Added</returns>
        private byte[] AddMessageHeader
            (MessageType msgType, byte[] messageBlob)
        {
            uint messageValue = 0;
            uint messageStatus = 0;
            uint reserveField = 0;
            bool requiresCheckSum = false;
            messageValue = (uint)msgType;
            switch (msgType)
            {
                case MessageType.CPMConnectIn:
                case MessageType.CPMCreateQueryIn:
                case MessageType.CPMGetRowsIn:
                case MessageType.CPMSetBindingsIn:
                case MessageType.CPMFetchValueIn:
                    requiresCheckSum = true;
                    break;
                default:
                    break;
            }
            int index = 0;
            uint checksum = 0;
            byte[] messagewithHeader = null;

            //If checksum is required
            //Calculate the checksum and assign the value
            if (requiresCheckSum)
            {
                if (messageBlob.Length % OFFSET_4 != 0)
                {
                    Array.Resize
                        (ref messageBlob,
                        messageBlob.Length
                        + (4 - messageBlob.Length % OFFSET_4));
                }

                while (index != messageBlob.Length)
                {
                    checksum += Helper.GetUInt(messageBlob, ref index);
                }

                checksum = checksum ^ CHECKSUM_XOR_VALUE;
                checksum -= messageValue;

            }
            index = 0;
            //Total message size
            messagewithHeader
                = new byte[4 * Constant.SIZE_OF_UINT + messageBlob.Length];
            Helper.CopyBytes
                (messagewithHeader, ref index,
                BitConverter.GetBytes(messageValue));
            Helper.CopyBytes
                (messagewithHeader, ref index,
                BitConverter.GetBytes(messageStatus));
            Helper.CopyBytes
                (messagewithHeader, ref index,
                BitConverter.GetBytes(checksum));
            Helper.CopyBytes
                (messagewithHeader, ref index,
                BitConverter.GetBytes(reserveField));
            Helper.CopyBytes(messagewithHeader, ref index, messageBlob);

            return messagewithHeader;
        }

        /// <summary>
        /// Returns the Storage SIZE of a given BaseStorageVariant type
        /// </summary>
        /// <param name="type">StorageType</param>
        /// <returns>size in bytes</returns>
        private ushort GetSize(StorageType type)
        {
            ushort size = 0;
            switch (type)
            {
                case StorageType.VT_EMPTY:
                    break;
                case StorageType.VT_NULL:
                    break;
                case StorageType.VT_I1:
                case StorageType.VT_UI1:
                    size = 1; // Take 1 Byte
                    break;
                case StorageType.VT_I2:
                case StorageType.VT_UI2:
                case StorageType.VT_BOOL:
                    size = 2; // Take 2 Bytes
                    break;
                case StorageType.VT_I4:
                case StorageType.VT_UI4:
                case StorageType.VT_INT:
                case StorageType.VT_UINT:
                case StorageType.VT_ERROR:
                case StorageType.VT_R4:
                    size = 4; // Take 4 byte
                    break;
                case StorageType.VT_I8:
                case StorageType.VT_UI8:
                case StorageType.VT_CY:
                case StorageType.VT_R8:
                case StorageType.VT_DATE:
                case StorageType.VT_CLSID:
                case StorageType.VT_FILETIME:
                    size = 8;
                    break;

                case StorageType.VT_DECIMAL:
                    size = 12;
                    break;
                case StorageType.VT_VARIANT:
                    if (this.Is64bit)
                    {
                        size = 24;
                    }
                    else
                    {
                        size = 16;
                    }
                    break;
                default:
                    break;
            }
            return size;
        }

        private byte[] ToBytes(IWspInMessage obj)
        {
            var buffer = new WspBuffer();

            obj.ToBytes(buffer);

            return buffer.GetBytes();
        }
        #endregion
    }

    /// <summary>
    /// Represent the Table Column to be queried
    /// </summary>
    public struct TableColumn
    {
        /// <summary>
        /// Guid of the Column
        /// </summary>
        public Guid Guid;
        /// <summary>
        /// Property Id of the column
        /// </summary>
        public uint PropertyId;
        /// <summary>
        /// Base Storage Type of the Column
        /// </summary>
        public StorageType Type;
        /// <summary>
        /// Length Offset of the Column
        /// </summary>
        public ushort LengthOffset;
        /// <summary>
        /// Value offset of the column
        /// </summary>
        public ushort ValueOffset;
        /// <summary>
        /// Status Offset of the Column
        /// </summary>
        public ushort StatusOffset;
    }

    /// <summary>
    /// ColumnId Class representing DBColumnId structure
    /// </summary>
    internal class ColumnId
    {
        /// <summary>
        /// Seek Kind for ConnectIn message
        /// </summary>
        public Ekind eKind;
        /// <summary>
        /// Table Column Guid
        /// </summary>
        public Guid guid;
        /// <summary>
        /// Specifies Property type (Name or ID )
        /// </summary>
        public uint UlId;
        /// <summary>
        /// Name of the Property
        /// </summary>
        public string propertyName = string.Empty;
        /// <summary>
        /// Property Id
        /// </summary>
        public uint propertyId = 0;
    }
}