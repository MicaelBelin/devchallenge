using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge
{
    /// <summary>
    /// Represents the connection to the server.
    /// </summary>
  
    public interface IConnection : IDisposable
    {
 
        /// <summary>
        /// Closes the connection. The connection cannot be reused after this method is called.
        /// </summary>
        void Close();

        /// <summary>
        /// Sends the request and immediately returns. Does not wait for a response.
        /// </summary>
        /// <param name="message"></param>
        void PostRequest(XElement message);

        /// <summary>
        /// Sends the request and waits for the response.
        /// This method blocks until the response is received.
        /// </summary>
        /// <param name="message">Specifies the message to send</param>
        /// <returns>The response of the request</returns>
        XElement SendRequest(XElement message);

        /// <summary>
        /// Sends the request and waits for the response.
        /// This method blocks until the response is received.
        /// If timeout is expired, a TimeoutException is thrown
        /// </summary>
        /// <param name="message">Specifies the message to send</param>
        /// <param name="timeout">Specifies the timespan until a TimeoutException is thrown</param>
        /// <returns>The response of the request</returns>
        XElement SendRequest(XElement message, TimeSpan timeout);

        /// <summary>
        /// Sends the specified notification.
        /// </summary>
        /// <param name="message"></param>
        void SendNotification(XElement message);

        /// <summary>
        /// Send the specified response for the specified request
        /// </summary>
        /// <param name="response"></param>
        /// <param name="requestid"></param>
        void SendResponse(XElement response, string requestid);

        
        /// <summary>
        /// Blocking. Waits for a request and returns first when a request is received and the filter has returned true.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IRequest WaitForRequest(Func<IRequest, FilterResponse> filter);

        /// <summary>
        /// Blocking. Waits for a request and returns first when a request is received and the filter has returned true.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        IRequest WaitForRequest(Func<IRequest, FilterResponse> filter, TimeSpan timeout);


        /// <summary>
        /// Blocking. Waits until a notification is received and filter returns true on it.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        INotification WaitForNotification(Func<INotification, FilterResponse> filter);


        /// <summary>
        /// Blocking. Waits until a notification is received and filter returns true on it.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        INotification WaitForNotification(Func<INotification, FilterResponse> filter, TimeSpan timeout);


        /// <summary>
        /// Registers a new filter.
        /// Once a filter is registered, it is placed on top of a filter stack. Each time a message is received, the filter is
        /// propagated through the filter stack, top first. If a filter returns FilterResponse.PassToNext, the filter below will be called with the message.
        /// If a filter returns FilterResponse.Consume, it halts the chain and the message is considered handled.
        /// 
        /// </summary>
        /// <param name="me"></param>
        void RegisterFilter(Func<XElement, FilterResponse> me);
        /// <summary>
        /// Unregisters the previously registered filter from filter stack
        /// </summary>
        /// <param name="me"></param>
        void UnregisterFilter(Func<XElement, FilterResponse> me);

        /// <summary>
        /// A monitor is a read-only function which is called on each message received. 
        /// All monitors are called with the message as a parameter, before the message are passed on to the filters.
        /// Monitors are primarilly used for debugging and statistics usage.
        /// </summary>
        /// <param name="me">Specifies the action to be called.</param>
        void RegisterMonitor(Action<XElement,MessageDirection> me);
        void UnregisterMonitor(Action<XElement,MessageDirection> me);


        /// <summary>
        /// Starts the message loop and runs indefinately.
        /// <remarks>
        /// Most usually Exec is finished with a thrown ClosedException, once the connection is closed. 
        /// Make sure you catch this exception and not treat it as an error.
        /// </remarks>
        /// <seealso cref="void RunUntilClosed()"/>
        /// </summary>
        void Exec();
        /// <summary>
        /// Runs the message loop for the specified period of time.
        /// </summary>
        /// <param name="timeout"></param>
        void Exec(TimeSpan timeout);
        /// <summary>
        /// Runs the message loop until the specified condition function returns false.
        /// The condition function is evaluated directly on start, then each time after a message has been received.
        /// </summary>
        /// <param name="condition"></param>
        void ExecWhile(Func<bool> condition);
        /// <summary>
        /// Executes until the specified condition function returns false, or timeout expires.
        /// If timeout expires, a TimeoutException is thrown.
        /// </summary>
        /// <seealso cref="void ExecWhile(Func<bool> condition)"/>
        /// <param name="condition"></param>
        /// <param name="timeout"></param>
        void ExecWhile(Func<bool> condition, TimeSpan timeout);

        /// <summary>
        /// Runs the message loop until the connection is closed, then returns discretly.
        /// This function consumes the ClosedException thrown but passes any other thrown exceptions.
        /// </summary>
        void RunUntilClosed();

        /// <summary>
        /// Converts the raw message into an IRequest structure.
        /// If the message is not of type Request, InvalidMessageTypeException is thrown.
        /// </summary>
        /// <param name="rawmsg"></param>
        /// <returns></returns>
        IRequest GetRequest(XElement rawmsg);
        /// <summary>
        /// Converts the raw message into an INotification structure.
        /// If the message is not of type Notification, InvalidMessageTypeException is thrown.
        /// </summary>
        /// <param name="rawmsg"></param>
        /// <returns></returns>
        INotification GetNotification(XElement rawmsg);
        /// <summary>
        /// Converts the raw message into an IResponse structure.
        /// If the message is not of type Response, InvalidMessageTypeException is thrown.
        /// </summary>
        /// <param name="rawmsg"></param>
        /// <returns></returns>
        IResponse GetResponse(XElement rawmsg);

        /// <summary>
        /// Creates an IRequest structure with the specified message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        IRequest CreateRequest(XElement message);

        /// <summary>
        /// Creates an INotification structure with the speficied message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        INotification CreateNotification(XElement message);

        /// <summary>
        /// Creates an IResponse structure with the specified message and requestid
        /// </summary>
        /// <param name="message"></param>
        /// <param name="requestid"></param>
        /// <returns></returns>
        IResponse CreateResponse(XElement message, string requestid);



    }


    public enum FilterResponse
    {
        Consume,
        PassToNext
    };

    public enum MessageDirection
    {
        Incoming,
        Outgoing,
    }


}
