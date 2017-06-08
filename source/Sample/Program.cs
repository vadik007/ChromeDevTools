using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;

namespace MasterDevs.ChromeDevTools.Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                // STEP 1 - Run Chrome
                var chromeProcessFactory = new ChromeProcessFactory();
                using (var chromeProcess = chromeProcessFactory.Create(9222))
                {
                    // STEP 2 - Create a debugging session
                    var endpointUrl = chromeProcess.GetSessions().Result.LastOrDefault();
                    var chromeSessionFactory = new ChromeSessionFactory();
                    var chromeSession = chromeSessionFactory.Create(endpointUrl);

                    // STEP 4 - Register for events (in this case, "Page" domain events)
                    // send an event to tell chrome to send us all Page events
                    // but we only subscribe to certain events in this session
                    var pageEnableResult = chromeSession.SendAsync<ChromeDevTools.Protocol.Chrome.Page.EnableCommand>().Result;
                    Console.WriteLine("PageEnable: " + pageEnableResult.Id);

                    List<string> frameIds = new List<string>();

                    chromeSession.Subscribe<Protocol.Chrome.Page.FrameAttachedEvent>(frameAttachedEvent =>
                    {
                        Console.WriteLine($"DomContentEvent: {frameAttachedEvent.FrameId} parent:{frameAttachedEvent.ParentFrameId}");
                        frameIds.Add(frameAttachedEvent.FrameId);
                    });

                    chromeSession.Subscribe<Protocol.Chrome.Page.FrameDetachedEvent>(frameDetachedEvent =>
                    {
                        Console.WriteLine($"FrameDetachedEvent: {frameDetachedEvent.FrameId}");
                        if (frameIds.Contains(frameDetachedEvent.FrameId))
                        {
                            frameIds.Remove(frameDetachedEvent.FrameId);
                        }
                    });

                    chromeSession.Subscribe<Protocol.Chrome.Page.DomContentEventFiredEvent>(domContentEvent =>
                    {
                        Console.WriteLine("DomContentEvent: " + domContentEvent.Timestamp);
                    });
                    // you might never see this, but that's what an event is ... right?
                    chromeSession.Subscribe<Protocol.Chrome.Page.FrameStartedLoadingEvent>(frameStartedLoadingEvent =>
                    {
                        Console.WriteLine("FrameStartedLoading: " + frameStartedLoadingEvent.FrameId);
                    });

                    // you might never see this, but that's what an event is ... right?
                    chromeSession.Subscribe<Protocol.Chrome.Page.FrameStoppedLoadingEvent>(frameStartedLoadingEvent =>
                    {
                        Console.WriteLine("FrameStartedLoading: " + frameStartedLoadingEvent.FrameId);
                        frameIds.Add(frameStartedLoadingEvent.FrameId);
                    });


                    // STEP 3 - Send a command
                    //
                    // Here we are sending a command to tell chrome to navigate to
                    // the specified URL
                    var navigateResponse = chromeSession.SendAsync(new NavigateCommand
                    {
                        Url = "http://www.yandex.ru"
                    })
                    .Result;
                    Console.WriteLine("NavigateResponse: " + navigateResponse.Id);
                    Thread.Sleep(3000);

                    var sendAsync = chromeSession.SendAsync(new SetDocumentContentCommand() { FrameId = frameIds.Last(), Html = "Hello" });

                    //foreach (var id in frameIds)
                    //    chromeSession.SendAsync(new SetDocumentContentCommand()
                    //    {
                    //        FrameId = id,
                    //        Html = "Hello"
                    //    });
                }

                //var SetDocumentContentResponce = chromeSession.SendAsync(new SetDocumentContentCommand()
                //    {
                //        FrameId = frameid,
                //        Html = "Hello"
                //    })
                //.Result;
                //Console.WriteLine("GetDocumentCommand: " + navigateResponse);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}