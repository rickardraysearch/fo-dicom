// Copyright (c) 2012-2019 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System.Threading.Tasks;

namespace Dicom.Network.Client
{
    /// <summary>
    /// Delegate for client handling the C-ECHO request immediately. This is used when the DicomClient serves as an SCP for
    /// some SOP Classes in an association. See http://dicom.nema.org/medical/dicom/current/output/chtml/part07/sect_D.3.3.4.html
    /// for details on the SCP/SCU Role Selection.
    /// </summary>
    /// <param name="request">C-ECHO request subject to handling.</param>
    /// <returns>Response from handling the C-ECHO <paramref name="request"/>.</returns>
    public delegate Task<DicomCEchoResponse> DicomClientCEchoRequestHandler(DicomCEchoRequest request);
}
