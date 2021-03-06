﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace TrailingWhitespace
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    class TextviewCreationListener : IVsTextViewCreationListener, IWpfTextViewConnectionListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public SVsServiceProvider serviceProvider { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            DTE2 dte = serviceProvider.GetService(typeof(DTE)) as DTE2;
            IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            textView.Properties.GetOrCreateSingletonProperty(() => new WhitespaceRemoverCommand(textViewAdapter, textView, dte));

            ITextDocument doc;
            if (TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out doc))
            {
                textView.Properties.GetOrCreateSingletonProperty(() => new RemoveWhitespaceOnSave(textViewAdapter, textView, dte, doc));
            }
        }

        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, System.Collections.ObjectModel.Collection<ITextBuffer> subjectBuffers)
        {
            foreach (var buffer in subjectBuffers)
            {
                TrailingClassifier classifier;
                if (buffer.Properties.TryGetProperty(typeof(TrailingClassifier), out classifier))
                {
                    classifier.SetTextView(textView);
                }
            }
        }

        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, System.Collections.ObjectModel.Collection<ITextBuffer> subjectBuffers)
        { }
    }
}
