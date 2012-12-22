SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;


CREATE TABLE IF NOT EXISTS `t_consumer` (
  `consumer_id` varchar(100) NOT NULL,
  `consumer_name` varchar(100) NOT NULL,
  `message_type` varchar(100) NOT NULL,
  `dest_url` varchar(2000) NOT NULL,
  PRIMARY KEY (`consumer_id`),
  UNIQUE KEY `t_consumer_message_type_idx` (`message_type`,`consumer_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `t_message` (
  `message_id` varchar(100) NOT NULL,
  `message_type` varchar(100) NOT NULL,
  `sent_at` varchar(100) NOT NULL,
  PRIMARY KEY (`message_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `t_message_delivery_log` (
  `record_id` varchar(100) NOT NULL,
  `message_id` varchar(100) NOT NULL,
  `consumer_id` varchar(100) NOT NULL,
  `status_code` varchar(100) NOT NULL,
  `response_content` text NOT NULL,
  `next_retry` varchar(100) NOT NULL,
  `last_delivery_attempt` varchar(100) NOT NULL,
  `retry_count` text NOT NULL,
  PRIMARY KEY (`message_id`,`consumer_id`),
  UNIQUE KEY `log_message_record_id_idx` (`record_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `t_message_values` (
  `message_id` varchar(100) NOT NULL,
  `name` varchar(750) NOT NULL,
  `value` text NOT NULL,
  KEY `message_id` (`message_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
